using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using LitJWT;
using LitJWT.Algorithms;
using Newtonsoft.Json;
using ZodiacBuddy.Stages.Brave;
using ZodiacBuddy.Stages.Novus;

namespace ZodiacBuddy.BonusLight;

/// <summary>
/// Manager for tracking bonus light.
/// </summary>
internal class BonusLightManager : IDisposable
{
    private const string BaseUri = "https://zodiac-buddy-db.fly.dev";
    private readonly JwtEncoder encoder;
    private readonly HttpClient httpClient;
    private readonly Timer resetTimer;
    private Timer? checkTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BonusLightManager"/> class.
    /// </summary>
    public BonusLightManager()
    {
        this.encoder = new JwtEncoder(new HS256Algorithm(Encoding.UTF8.GetBytes(Secrets.JwtSecret)));
        this.httpClient = new HttpClient();

        var timeOfDay = DateTime.UtcNow.TimeOfDay;
        var nextEvenHour = timeOfDay.Hours % 2 == 0 ?
            TimeSpan.FromHours(timeOfDay.Hours + 2) :
            TimeSpan.FromHours(timeOfDay.Hours + 1);
        var delta = nextEvenHour - timeOfDay;

        Service.ClientState.Login += this.OnLogin;
        Service.ClientState.Logout += this.OnLogout;
        if (Service.ClientState.LocalPlayer is not null) this.OnLogin(null, null!);
        this.resetTimer = new Timer(_ => this.ResetBonus(), null, delta, TimeSpan.FromHours(2));
    }

    private static BonusLightConfiguration LightConfiguration => Service.Configuration.BonusLight;

    /// <inheritdoc/>
    public void Dispose()
    {
        Service.ClientState.Login -= this.OnLogin;
        Service.ClientState.Logout -= this.OnLogout;
        this.resetTimer?.Dispose();
        this.checkTimer?.Dispose();
        this.httpClient?.Dispose();
    }

    /// <summary>
    /// Update the bonus light configuration and play any notifications required.
    /// </summary>
    /// <param name="territoryId">Territory ID.</param>
    /// <param name="date">Date.</param>
    /// <param name="message">Message to display.</param>
    public void UpdateLightBonus(uint? territoryId, DateTime? date, string? message)
    {
        LightConfiguration.LightBonusTerritoryId = territoryId;
        LightConfiguration.LightBonusDetection = date;
        Service.Configuration.Save();

        if (message == null)
            return;

        if (LightConfiguration.NotifyLightBonusOnlyWhenEquipped)
        {
            var mainhand = Util.GetEquippedItem(0);
            var offhand = Util.GetEquippedItem(1);

            if (NovusRelic.Items.ContainsKey(mainhand.ItemID) ||
                NovusRelic.Items.ContainsKey(offhand.ItemID) ||
                BraveRelic.Items.ContainsKey(mainhand.ItemID) ||
                BraveRelic.Items.ContainsKey(offhand.ItemID))
            {
                return;
            }
        }

        Service.Plugin.PrintMessage(message);

        if (LightConfiguration.PlaySoundOnLightBonusNotification)
        {
            var soundId = (uint)LightConfiguration.LightBonusNotificationSound;
            UIModule.PlayChatSoundEffect(soundId);
        }
    }

    /// <summary>
    /// Send a new bonus light event to the server.
    /// </summary>
    /// <param name="territoryId">Id of the duty with the light bonus.</param>
    public void SendReport(uint territoryId)
    {
        if (Service.ClientState.LocalPlayer == null)
            return;

        if (Service.ClientState.LocalPlayer.HomeWorld.GameData == null)
            return;

        var datacenter = Service.ClientState.LocalPlayer.HomeWorld.GameData.DataCenter.Row;
        var world = Service.ClientState.LocalPlayer.HomeWorld.Id;

        var report = new Report(datacenter, world, territoryId, DateTime.UtcNow);
        var content = JsonConvert.SerializeObject(report);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUri}/reports/");
        request.Headers.Add("x-access-token", this.GenerateJWT());
        request.Content = new StringContent(content, Encoding.UTF8, "application/json");

        this.Send(request, null);
    }

    private void CheckBonus()
    {
        if (LightConfiguration.LightBonusDetection == null)
            this.RetrieveLastReport();
    }

    private void ResetBonus()
    {
        this.UpdateLightBonus(null, null, null);
    }

    /// <summary>
    /// Retrieve the last report about light bonus for the current datacenter.
    /// </summary>
    private void RetrieveLastReport()
    {
        if (Service.ClientState.LocalPlayer == null)
            return;

        if (Service.ClientState.LocalPlayer.HomeWorld.GameData == null)
            return;

        var datacenter = Service.ClientState.LocalPlayer.HomeWorld.GameData.DataCenter.Row;

        if (datacenter == 0)
            return;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}/reports/last/{datacenter}");

        this.Send(request, this.OnLastReportResponse);
    }

    private void OnLogin(object? sender, EventArgs e)
    {
        this.checkTimer?.Dispose();
        this.checkTimer = new Timer(_ => this.CheckBonus(), null, TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(5));
    }

    private void OnLogout(object? sender, EventArgs e)
    {
        this.checkTimer?.Dispose();
        this.ResetBonus();
    }

    private void OnLastReportResponse(string content)
    {
        var report = JsonConvert.DeserializeObject<Report>(content);
        if (this.ReportStillActive(report) && BonusLightDuty.TryGetValue(report.TerritoryId, out var territoryLight))
        {
            var message = $"Light bonus detected on \"{territoryLight?.DutyName}\"";
            this.UpdateLightBonus(report.TerritoryId, report.Date, message);
        }
    }

    private bool ReportStillActive(Report report)
    {
        var timeOfDay = DateTime.UtcNow.TimeOfDay;
        var lastEvenHour = timeOfDay.Hours % 2 == 0
            ? TimeSpan.FromHours(timeOfDay.Hours - 2)
            : TimeSpan.FromHours(timeOfDay.Hours - 1);
        var deltaSinceReport = report.Date.ToUniversalTime().TimeOfDay - lastEvenHour;

        // Still need to check DT for the day.
        var dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(2));

        return report.Date >= dt && deltaSinceReport.TotalSeconds > 0;
    }

    private void Send(HttpRequestMessage request, Action<string>? successCallback)
    {
        Task.Run(async () =>
        {
            var response = this.httpClient.Send(request);
            var content = await response.Content.ReadAsStringAsync();
            PluginLog.Verbose($"{request.RequestUri} => [{response.StatusCode:D}] {content}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode != HttpStatusCode.NotFound)
                    PluginLog.Warning($"{request.RequestUri} => [{response.StatusCode:D}] {content}");

                return;
            }

            successCallback?.Invoke(content);
        });
    }

    private string GenerateJWT()
    {
        var payload = new Dictionary<string, object>
        {
            { "sub", Service.ClientState.LocalContentId },
            { "aud", "ZodiacBuddy" },
            { "iss", "ZodiacBuddyDB" },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
        };

        return this.encoder.Encode(payload, TimeSpan.FromMinutes(15));
    }
}