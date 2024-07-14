using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
public class BonusLightManager : IDisposable {
    private const string BaseUri = "https://zodiac-buddy-db.fly.dev";
    private readonly JwtEncoder encoder;
    private readonly HttpClient httpClient;
    private readonly Timer resetTimer;
    private Timer? checkTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BonusLightManager"/> class.
    /// </summary>
    public BonusLightManager() {
        // Congrats, you found the secret key.
        var algoKey = Encoding.UTF8.GetBytes(
            "BE11C9E53416BB9B9FB99B33C" +
            "5B8AF0FA6A55CABB3F33774E3" +
            "437AE83BF4E8DB");

        this.encoder = new JwtEncoder(new HS256Algorithm(algoKey));
        this.httpClient = new HttpClient();

        var timeOfDay = DateTime.UtcNow.TimeOfDay;
        var nextEvenHour = timeOfDay.Hours % 2 == 0
            ? TimeSpan.FromHours(timeOfDay.Hours + 2)
            : TimeSpan.FromHours(timeOfDay.Hours + 1);
        var delta = nextEvenHour - timeOfDay;

        Service.ClientState.Login += this.OnLogin;
        Service.ClientState.Logout += this.OnLogout;
        if (Service.ClientState.LocalPlayer is not null) this.OnLogin();
        this.resetTimer = new Timer(_ => this.ResetBonus(), null, delta, TimeSpan.FromHours(2));
    }

    /// <summary>
    /// Gets a value indicating whether if the last request was successful.
    /// </summary>
    public bool LastRequestIsSuccess { get; private set; } = true;

    private static BonusLightConfiguration LightConfiguration
        => Service.Configuration.BonusLight;

    /// <inheritdoc/>
    public void Dispose() {
        Service.ClientState.Login -= this.OnLogin;
        Service.ClientState.Logout -= this.OnLogout;
        this.resetTimer.Dispose();
        this.checkTimer?.Dispose();
        this.httpClient.Dispose();
    }

    /// <summary>
    /// Update the bonus light configuration and play any notifications required.
    /// </summary>
    /// <param name="territoryId">Territory ID.</param>
    /// <param name="detectionTime">DateTime of the detection.</param>
    /// <param name="onDutyFromBeginning">Define if the player has join an ongoing duty or has reconnected.</param>
    /// <param name="message">Message to display.</param>
    public void AddLightBonus(uint territoryId, DateTime? detectionTime, bool onDutyFromBeginning, string message) {
        if (LightConfiguration.ActiveBonus.Contains(territoryId))
            return;

        this.NotifyLightBonus([message]);

        // Don't report/add incomplete duty
        if (!onDutyFromBeginning || detectionTime == null)
            return;

        // Don't report/add past bonus
        // Remove 5s to ignore the 5 first second of a new window
        // (Reduce false positive due to networking/loading)
        var reportTime = ((DateTime)detectionTime).AddSeconds(-5);
        if (!this.ReportStillActive(reportTime))
            return;

        LightConfiguration.ActiveBonus.Add(territoryId);

        this.SendReport(territoryId, (DateTime)detectionTime);
    }

    /// <summary>
    /// Send a new bonus light event to the server.
    /// </summary>
    /// <param name="territoryId">Id of the duty with the light bonus.</param>
    /// <param name="detectionTime">DateTime of the detection.</param>
    private void SendReport(uint territoryId, DateTime detectionTime) {
        if (Service.ClientState.LocalPlayer == null)
            return;

        if (Service.ClientState.LocalPlayer.HomeWorld.GameData == null)
            return;

        var datacenter = Service.ClientState.LocalPlayer.HomeWorld.GameData.DataCenter.Row;
        var world = Service.ClientState.LocalPlayer.HomeWorld.Id;

        var report = new Report(datacenter, world, territoryId, detectionTime);
        var content = JsonConvert.SerializeObject(report);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUri}/reports/");
        request.Headers.Add("x-access-token", this.GenerateJWT());
        request.Content = new StringContent(content, Encoding.UTF8, "application/json");

        this.Send(request, null);
    }

    /// <summary>
    /// Play any notifications required.
    /// </summary>
    /// <param name="message">Message to display.</param>
    private void NotifyLightBonus(string[] message) {
        if (LightConfiguration.NotifyLightBonusOnlyWhenEquipped) {
            var mainHand = Util.GetEquippedItem(0);
            var offHand = Util.GetEquippedItem(1);

            if (!NovusRelic.Items.ContainsKey(mainHand.ItemId) &&
                !NovusRelic.Items.ContainsKey(offHand.ItemId) &&
                !BraveRelic.Items.ContainsKey(mainHand.ItemId) &&
                !BraveRelic.Items.ContainsKey(offHand.ItemId)) {
                return;
            }
        }

        foreach (var s in message) {
            Service.Plugin.PrintMessage(s);
        }

        if (LightConfiguration.PlaySoundOnLightBonusNotification) {
            var soundId = (uint)LightConfiguration.LightBonusNotificationSound;
            UIModule.PlayChatSoundEffect(soundId);
        }
    }

    private void ResetBonus()
        => LightConfiguration.ActiveBonus.Clear();

    /// <summary>
    /// Retrieve the last report about light bonus for the current datacenter.
    /// </summary>
    private void RetrieveLastReport() {
        Service.Framework.RunOnFrameworkThread(() => {
            if (Service.ClientState.LocalPlayer == null)
                return;

            if (Service.ClientState.LocalPlayer.HomeWorld.GameData == null)
                return;

            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}/reports/active");

            this.Send(request, this.OnLastReportResponse);
        });
    }

    private void OnLogin() {
        this.checkTimer?.Dispose();
        this.checkTimer =
            new Timer(_ => this.RetrieveLastReport(), null, TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(5));
    }

    private void OnLogout() {
        this.checkTimer?.Dispose();
        this.ResetBonus();
    }

    private void OnLastReportResponse(string content) {
        var reports = JsonConvert.DeserializeObject<List<Report>>(content);
        if (reports == null || reports.Count == 0)
            return;

        var listUpdated = new List<string> { "New light bonus detected" };
        foreach (Report report in reports) {
            if (this.ReportStillActive(report.Date) &&
                BonusLightDuty.TryGetValue(report.TerritoryId, out var duty) &&
                !LightConfiguration.ActiveBonus.Contains(report.TerritoryId)) {
                LightConfiguration.ActiveBonus.Add(report.TerritoryId);
                listUpdated.Add($" {duty!.DutyName}"); // This '' is an arrow in game
            }
        }

        if (listUpdated.Count > 1) {
            this.NotifyLightBonus(listUpdated.ToArray());
        }
    }

    private bool ReportStillActive(DateTime reportDateTime) {
        var timeOfDay = DateTime.UtcNow.TimeOfDay;
        var lastEvenHour = timeOfDay.Hours % 2 == 0
            ? TimeSpan.FromHours(timeOfDay.Hours)
            : TimeSpan.FromHours(timeOfDay.Hours - 1);
        var deltaSinceLastEvenHour = timeOfDay - lastEvenHour;
        return reportDateTime >= DateTime.UtcNow.Subtract(deltaSinceLastEvenHour);
    }

    private void Send(HttpRequestMessage request, Action<string>? successCallback) {
        Task.Run(async () => {
            try {
                var response = await this.httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                this.LastRequestIsSuccess = response.IsSuccessStatusCode;
                if (!response.IsSuccessStatusCode) {
                    Service.PluginLog.Warning($"{request.RequestUri} => [{response.StatusCode:D}] {content}");
                    return;
                }

                Service.PluginLog.Verbose($"{request.RequestUri} => [{response.StatusCode:D}] {content}");
                successCallback?.Invoke(content);
            }
            catch (HttpRequestException e) {
                Service.PluginLog.Error($"{request.RequestUri} => {e}");
                this.LastRequestIsSuccess = false;
            }
        });
    }

    private string GenerateJWT() {
        var payload = new Dictionary<string, object> {
            { "sub", Service.ClientState.LocalContentId },
            { "aud", "ZodiacBuddy" },
            { "iss", "ZodiacBuddyDB" },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "version", 3 }, // message version to compare with the server
        };

        return this.encoder.Encode(payload, TimeSpan.FromMinutes(15));
    }
}