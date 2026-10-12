namespace Mattermost.Enums
{
    /// <summary>Native status updates supported by the job-management endpoint.</summary>
    public enum JobStatusUpdate
    {
        /// <summary>Return a job to the pending queue.</summary>
        Pending,
        /// <summary>Request cancellation; pending jobs become canceled immediately.</summary>
        CancelRequested,
        /// <summary>Mark a job as canceled.</summary>
        Canceled
    }
}
