using Mattermost.Constants;
using Mattermost.Models.Teams;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Get team by specified identifier.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <returns> Team information. </returns>
        public Task<Team> GetTeamAsync(string teamId)
        {
            return SendRequestAsync<Team>(HttpMethod.Get, Routes.Teams + "/" + teamId);
        }
    }
}
