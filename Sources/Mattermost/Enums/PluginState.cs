namespace Mattermost.Enums
{
    /// <summary>Native numeric plugin runtime states.</summary>
    public enum PluginState
    {
        /// <summary>Plugin is not running.</summary>
        NotRunning = 0,
        /// <summary>Plugin is starting; reserved by the server.</summary>
        Starting = 1,
        /// <summary>Plugin is running.</summary>
        Running = 2,
        /// <summary>Plugin failed to start.</summary>
        FailedToStart = 3,
        /// <summary>Plugin stopped unexpectedly.</summary>
        FailedToStayRunning = 4,
        /// <summary>Plugin is stopping; reserved by the server.</summary>
        Stopping = 5
    }
}
