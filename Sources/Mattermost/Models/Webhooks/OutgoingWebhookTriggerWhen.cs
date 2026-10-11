namespace Mattermost.Models.Webhooks
{
    /// <summary>Outgoing webhook trigger matching mode.</summary>
    public enum OutgoingWebhookTriggerWhen
    {
        /// <summary>The first word exactly matches a trigger word.</summary>
        FirstWordExactMatch = 0,

        /// <summary>The first word starts with a trigger word.</summary>
        FirstWordStartsWith = 1
    }
}
