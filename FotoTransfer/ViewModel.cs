namespace FotoTransfer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Input;

    /// <summary>
    /// The view model class of the FotoTransfer project
    /// </summary>
    class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The commands (represented by buttons)
        /// </summary>
        private ICommand closeCommand;
        private ICommand browseSourceCommand;
        private ICommand browseTargetCommand;
        private ICommand startCommand;

        /// <summary>
        /// Backup fields for the properties
        /// </summary>
        private string sourcePath;
        private string targetPath;
        private string information;
        private double progressPercentage;
        private TransferState transferState;
        private DateTime startDate;
        private DateTime endDate;

        /// <summary>
        /// The background task for file search and copy
        /// </summary>
        private Task fileHandlerTask;

        /// <summary>
        /// Creates a new instance of <see cref="ViewModel"/>
        /// </summary>
        public ViewModel()
        {
            // Read initial values from user settings
            this.SourcePath = Properties.Settings.Default.SourcePath;
            this.TargetPath = Properties.Settings.Default.TargetPath;
            this.EndDate = DateTime.Today;
            this.StartDate = new DateTime(2000, 1, 1);
        }

        /// <summary>
        /// Exports the close command so it can be bound to a button
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    // The close command gets an action passed that is defined here in the view.
                    this.closeCommand = new CloseCommand(param => this.OnRequestClose());
                }

                return this.closeCommand;
            }
        }

        /// <summary>
        /// The browse command for the source directory bound to the browse button
        /// </summary>
        public ICommand BrowseSourceCommand
        {
            get
            {
                if (this.browseSourceCommand == null)
                {
                    this.browseSourceCommand = new BrowseCommand(this.sourcePath, param => this.SetSourcePath(param));
                }

                return this.browseSourceCommand;
            }
        }

        /// <summary>
        /// The browse command for the target directory bound to the browse button
        /// </summary>
        public ICommand BrowseTargetCommand
        {
            get
            {
                if (this.browseTargetCommand == null)
                {
                    this.browseTargetCommand = new BrowseCommand(this.targetPath, param => this.SetTargetPath(param));
                }

                return this.browseTargetCommand;
            }
        }

        /// <summary>
        /// The start command bound to the start button
        /// </summary>
        public ICommand StartCommand
        {
            get
            {
                if (this.startCommand == null)
                {
                    // Start command with a start action and a canStart query
                    this.startCommand = new StartCommand(
                        () => this.Start(),
                        () => this.CanStart());
                }

                return this.startCommand;
            }
        }

        /// <summary>
        /// Gets the source path (bound to the source path text box)
        /// </summary>
        public String SourcePath
        {
            get
            {
                return this.sourcePath;
            }

            private set
            {
                if (this.sourcePath != value)
                {
                    this.sourcePath = value;
                    Properties.Settings.Default.SourcePath = value;
                    this.OnPropertyChanged("SourcePath");
                }
            }
        }

        /// <summary>
        /// Gets the target path (bound to the target path text box)
        /// </summary>
        public String TargetPath
        {
            get
            {
                return this.targetPath;
            }

            private set
            {
                if (this.targetPath != value)
                {
                    this.targetPath = value;
                    Properties.Settings.Default.TargetPath = value;
                    this.OnPropertyChanged("TargetPath");
                }
            }
        }

        /// <summary>
        /// Gets the information (bound to the information text box)
        /// </summary>
        public string Information
        {
            get
            {
                return this.information; 
            }

            private set
            {
                if (this.information != value)
                {
                    this.information = value;
                    this.OnPropertyChanged("Information");
                }
            }
        }

        public double ProgressPercentage
        {
            get { return this.progressPercentage; }

            private set
            {
                if (this.progressPercentage != value)
                {
                    this.progressPercentage = value;
                    this.OnPropertyChanged("ProgressPercentage");
                }
            }
        }
        /// <summary>
        /// Gets the start date (bound to the start date picker)
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return this.startDate;
            }

            private set
            {
                if (value > this.EndDate)
                {
                    throw new ArgumentException("Das Startdatum muss vor dem Endedatum liegen");
                }

                this.startDate = value;
                this.OnPropertyChanged("StartDate");
            }
        }

        /// <summary>
        /// Gets the end date (bound to the end date picker)
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return this.endDate;
            }

            private set
            {
                if (value < this.StartDate)
                {
                    throw new ArgumentException("Das Endedatum muss nach dem Startdatum liegen");
                }

                this.endDate = value;
                this.OnPropertyChanged("EndDate");
            }
        }

        #region Default INotifyPropertyChanged implementation
    
        // The event that can be fired if a property has changed
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The method that is called if any propery is changed.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region RequestClose [event]

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        /// <summary>
        /// Action that is performed by the CloseCommand when it is activated.
        /// </summary>
        /// <remarks>
        /// Since the code for this action is not coded here, we transfer the stuff to the RequestClose 
        /// event handler which is set in App.xaml.cs
        /// </remarks>
        private void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion // RequestClose [event]

        #region Browse

        /// <summary>
        /// Sets the source path (bound to the source path browse command)
        /// </summary>
        /// <param name="path">The source path.</param>
        private void SetSourcePath(string path)
        {
            this.SourcePath = path;
        }

        /// <summary>
        /// Sets the target path (bound to the target path browse command)
        /// </summary>
        /// <param name="path">The source path.</param>
        private void SetTargetPath(string path)
        {
            this.TargetPath = path;
        }

        #endregion

        #region Start

        /// <summary>
        /// The start action
        /// </summary>
        private void Start()
        {
            this.Information = "Fotos werden gesucht...";

            ExifFileHandler handler = new ExifFileHandler();

            // The progress object is passed to the worker thread. It updates the Information property
            // and thus raises a propertyChanged event which is used to display the background worker progress.
            IProgress<TransferProgress> progress = new Progress<TransferProgress>(this.ProgressHandler);

            // The background task uses the Task library instead of the BackgroundWorker (this would be old-school).
            // The progress handler is passed down to the method executed in the background thread of the task.
            // By a miracle the progress information is tunneled through the threads and triggers an update of the
            // information text in the GUI.
            this.fileHandlerTask = Task.Factory.StartNew(() => handler.FindAndCopyFiles(this.sourcePath, this.targetPath, this.startDate, this.endDate, progress));
        }

        /// <summary>
        /// The handler for progress information
        /// </summary>
        /// <param name="progress">The progress object</param>
        private void ProgressHandler(TransferProgress progress)
        {
            this.Information = progress.ProgressInfo;
            this.ProgressPercentage = progress.Percentage;

            if (progress.State != this.transferState)
            {
                // Force the command manager to re-query the commands.
                // This way the command button can see that progress state goes to "Done" and will be re-enabled.
                CommandManager.InvalidateRequerySuggested();
            }

            this.transferState = progress.State;
        }

        /// <summary>
        /// Determines whether all entries are correct to start the process
        /// </summary>
        /// <returns>true if all entries are correct, otherwise false.</returns>
        private bool CanStart()
        {
            if (string.IsNullOrEmpty(this.sourcePath))
            {
                this.Information = "Das Quellverzeichnis ist nicht angegeben";
                return false;
            }

            if (!Directory.Exists(this.SourcePath))
            {
                this.Information = "Das Quellverzeichnis existiert nicht";
                return false;
            }

            if (string.IsNullOrEmpty(this.targetPath))
            {
                this.Information = "Das Zielverzeichnis ist nicht angegeben";
                return false;
            }

            if (!Directory.Exists(this.targetPath))
            {
                this.Information = "Das Zielverzeichnis existiert nicht";
                return false;
            }

            if (this.startDate > this.endDate)
            {
                this.Information = "Das Startdatum muss vor dem Endedatum liegen";
                return false;
            }

            this.Information = string.Empty;

            // Greys out the start button if a background task is in progress.
            switch (this.transferState)
            {
                case TransferState.Searching:
                case TransferState.Copying:
                    return false;
            }

            return true;
        }

        #endregion
    }
}
