namespace FotoTransfer
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// A class for the start command
    /// </summary>
    class StartCommand : ICommand
    {
        /// <summary>
        /// The start action, passed in the constructor
        /// </summary>
        private readonly Action execute;

        /// <summary>
        /// The query if the command can execute
        /// </summary>
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Creates a new instance of the start command
        /// </summary>
        /// <param name="execute">The start action.</param>
        /// <param name="canExecute">The query whether the command can execute.</param>
        public StartCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Executes the start command
        /// </summary>
        /// <param name="parameter">Not used, but required by the interface.</param>
        public void Execute(object parameter)
        {
            this.execute?.Invoke();
        }

        /// <summary>
        /// Indicates whether the start command can execute.
        /// </summary>
        /// <param name="parameter">The parameter (not used)</param>
        /// <returns>True if the command can execute, otherwise false.</returns>
        public bool CanExecute(object parameter)
        {
            // CanExecute calls the canExecute func which has been inserted into the command by the view model.
            // In other words, the view model determines whether the command can execute.
            return ((this.execute != null) && this.canExecute());
        }

        /// <summary>
        /// Event handler for command execution validation
        /// </summary>
        /// <remarks>
        /// CanExecute is called by means of this event handler. Each control that is part of the CanExecute check
        /// must specify "UpdateSourceTrigger=PropertyChanged". That means that each time the property of the control
        /// changes, a requery is triggered.
        /// </remarks>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
