namespace Mattermost.Models.Requests.Websocket
{
    internal class ActionRequest
    {
        public int Seq { get; set; }
        public string Action { get; set; } = null!;
        public object Data { get; set; } = null!;
    }
}