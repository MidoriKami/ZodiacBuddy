using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Game.ClientState;
using Dalamud.Logging;
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
        private const string BaseUri = "https://sparkling-glade-8937.fly.dev";
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
            this.httpClient.Dispose();
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
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            this.Send(request, null);
        }

        private void Send(HttpRequestMessage request, Action<HttpResponseMessage>? callback)
        {
            Task.Run(() =>
            {
                var response = this.httpClient.Send(request);
                callback?.Invoke(response);
            });
        }

        private async void OnLastReportResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            PluginLog.Debug($"{response.StatusCode} => {content}");
            if (!response.IsSuccessStatusCode)
                return;

            var report = JsonConvert.DeserializeObject<Report>(content);
            var dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(2));
            if (report.Date >= dt && NovusDuty.TryGetValue(report.Territory, out var territoryLight))
            {
                var message = $"Light bonus detected on \"{territoryLight?.DutyName}\"";
                NovusManager.UpdateLightBonus(report.Territory, report.Date, message);
            }
        }
    }
}