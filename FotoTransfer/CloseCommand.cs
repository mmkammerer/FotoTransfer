namespace FotoTransfer
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// A class for the close command
    /// </summary>
    class CloseCommand : ICommand
    {
        /// <summary>
        /// The close action, passed in the constructor
        /// </summary>
        private readonly Action<object> execute;

        /// <summary>
        /// Creates a new instance of the close command
        /// </summary>
        /// <param name="execute">The close action.</param>
        public CloseCommand(Action<object> execute)
        {
            this.execute = execute;
        }

        /// <summary>
        /// Executes the close command
        /// </summary>
        /// <param name="parameter">Not used, but required by the interface.</param>
        public void Execute(object parameter)
        {
            this.execute?.Invoke(parameter);
        }

        /// <summary>
        /// Indicates whether a command can execute.
        /// </summary>
        /// <param name="parameter">The parameter (not used)</param>
        /// <returns>True.</returns>
        public bool CanExecute(object parameter)
        {
            return true;
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
