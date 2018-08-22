namespace FotoTransfer
{
    using System;
    using System.Windows.Forms;
    using System.Windows.Input;

    /// <summary>
    /// A class for the browse command
    /// </summary>
    class BrowseCommand : ICommand
    {
        /// <summary>
        /// The browse action, passed in the constructor
        /// </summary>
        private readonly Action<string> execute;

        private string path = string.Empty;

        /// <summary>
        /// Creates a new instance of the browse command
        /// </summary>
        /// <param name="initialPath">The initial path.</param>
        /// <param name="execute">The browse action.</param>
        public BrowseCommand(string initialPath, Action<string> execute)
        {
            this.path = initialPath;
            this.execute = execute;
        }

        /// <summary>
        /// Executes the browse command
        /// </summary>
        /// <param name="parameter">Not used, but required by the interface.</param>
        public void Execute(object parameter)
        {
            var dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = this.path
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.path = dialog.SelectedPath;
            }

            this.execute?.Invoke(this.path);
        }

        /// <summary>
        /// Indicates whether a command can execute.
        /// </summary>
        /// <param name="parameter">The parameter (not used)</param>
        /// <returns>True.</returns>
        public bool CanExecute(object parameter)
        {
            return this.execute != null;
        }

        /// <summary>
        /// Unused event handler.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
