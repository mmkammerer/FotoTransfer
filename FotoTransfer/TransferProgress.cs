namespace FotoTransfer
{
    using System;

    /// <summary>
    /// The state of the transfer
    /// </summary>
    public enum TransferState
    {
        /// <summary>
        /// Initial state
        /// </summary>
        Unknown,

        /// <summary>
        /// Searching for pictures
        /// </summary>
        Searching,

        /// <summary>
        /// Pictures are being copied
        /// </summary>
        Copying,

        /// <summary>
        /// Transfer is done
        /// </summary>
        Done
    };

    /// <summary>
    /// Holds the progress of the search and copy action
    /// </summary>
    public class TransferProgress
    {
        /// <summary>
        /// The state of the transfer
        /// </summary>
        public TransferState State { get; set; }

        /// <summary>
        /// Additional information about the transfer
        /// </summary>
        public string ProgressInfo { get; set; }

        /// <summary>
        /// The percentage of files that have been copied
        /// </summary>
        public Double Percentage { get; set; }
    }
}
