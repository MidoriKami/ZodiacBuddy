using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Game.ClientState;
using Dalamud.Logging;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json;
using ZodiacBuddy.Novus;
using ZodiacBuddy.Novus.Data;
using ZodiacBuddy.Stages.Novus.Data;

namespace ZodiacBuddy.Stages.Novus
{
    /// <summary>
    /// Http client to share and retrieve reports about duty with Novus bonus of light.
    /// </summary>
    internal class NovusHttpClient : IDisposable
    {
        private const string BaseUri = "https://zodiac-buddy-db.fly.dev";
        private readonly JwtEncoder encoder = new(new HMACSHA512Algorithm(), new JsonNetSerializer(), new JwtBase64UrlEncoder());
        private readonly HttpClient httpClient;
        private readonly ClientState clientState;

        /// <summary>
        /// Initializes a new instance of the <see cref="NovusHttpClient"/> class.
        /// </summary>
        public NovusHttpClient()
        {
            this.httpClient = new HttpClient();
            this.clientState = Service.ClientState;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.httpClient?.Dispose();
        }

        /// <summary>
        /// Retrieve the last report about light bonus for the current datacenter.
        /// </summary>
        public void RetrieveLastReport()
        {
            if (this.clientState.LocalPlayer == null)
                return;

            if (this.clientState.LocalPlayer.HomeWorld.GameData == null)
                return;

            var datacenter = this.clientState.LocalPlayer.HomeWorld.GameData.DataCenter.Row;

            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}/reports/last/{datacenter}");

            this.Send(request, this.OnLastReportResponse);
        }

        /// <summary>
        /// Send a new report to the server.
        /// </summary>
        /// <param name="territoryId">Id of the duty with the light bonus.</param>
        public void SendReport(uint territoryId)
        {
            if (this.clientState.LocalPlayer == null)
                return;

            if (this.clientState.LocalPlayer.HomeWorld.GameData == null)
                return;

            var datacenter = this.clientState.LocalPlayer.HomeWorld.GameData.DataCenter.Row;
            var world = this.clientState.LocalPlayer.HomeWorld.Id;

            var report = new Report(datacenter, world, territoryId, DateTime.UtcNow);
            var content = JsonConvert.SerializeObject(report);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUri}/reports/");
            request.Headers.Add("x-access-token", this.GenerateJWT());
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            this.Send(request, null);
        }

        private void Send(HttpRequestMessage request, Action<string>? successCallback)
        {
            Task.Run(async () =>
            {
                var response = this.httpClient.Send(request);
                var content = await response.Content.ReadAsStringAsync();
                PluginLog.Verbose($"{request.RequestUri} => [{response.StatusCode}] {content}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode != HttpStatusCode.NotFound)
                        PluginLog.Warning($"{request.RequestUri} => [{response.StatusCode}] {content}");

                    return;
                }

                successCallback?.Invoke(content);
            });
        }

        private void OnLastReportResponse(string content)
        {
            var report = JsonConvert.DeserializeObject<Report>(content);
            if (this.ReportStillActive(report) && NovusDuty.TryGetValue(report.TerritoryId, out var territoryLight))
            {
                var message = $"Light bonus detected on \"{territoryLight?.DutyName}\"";
                NovusManager.UpdateLightBonus(report.TerritoryId, report.Date, message);
            }
        }

        private string GenerateJWT()
        {
            var payload = new Dictionary<string, object>
            {
                { "sub", this.clientState.LocalContentId },
                { "aud", "ZodiacBuddy" },
                { "iss", "ZodiacBuddyDB" },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            };
            return this.encoder.Encode(payload, Secrets.JwtSecret);
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
    }
}