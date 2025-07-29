using System.Text.Json;
using Mattermost.Constants;
using System.Threading.Tasks;
using Mattermost.Models.Teams;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Get team by specified identifier.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <returns> Team information. </returns>
        public async Task<Team> GetTeamAsync(string teamId)
        {
            string url = Routes.Teams + "/" + teamId;
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Team>(json)
                ?? throw new JsonException("Failed to deserialize team information.");
        }
    }
}
