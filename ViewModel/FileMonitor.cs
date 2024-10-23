/******************************************************************************
* Filename    = FileMonitor.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater.Client
* 
* Project     = File Watcher
*
* Description = Notifies if any new analyzer file(dll) either added or deleted to the watching folder.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using FileWatcherMVVM1.ExternalServices;

namespace FileWatcherMVVM1.ViewModel
{
    /// <summary>
    /// The FileMonitor class monitors a specified folder for file creation and deletion events
    /// and raises property changes to update the UI with status messages.
    /// </summary>
    public class FileMonitor : INotifyPropertyChanged
    {
        //Holds the current status message
        private string _messageStatus;
        //Monitors the file system changes in the directory
        private FileSystemWatcher _fileWatcher;
        //Stores the list of created files
        private List<string> _createdFiles;
        //Stores the list of deleted files
        private List<string> _deletedFiles;
        //Timer to debounce file change events for batch processing
        private Timer _timer;
        //adding cloud sync service
        private CloudSyncService _cloudSyncService;

        /// <summary>
        /// Initializes a new instance of the FileMonitor class and starts monitoring the folder.
        /// </summary>
        public FileMonitor()
        {
            //Intialize the list for created and deleted files.
            _createdFiles = new List<string>();
            _deletedFiles = new List<string>();
            //Intialize the cloud service
            /*Needs to add connetion string part*/
            _cloudSyncService = new CloudSyncService("Azure_Connection_String", "our_container");
            //Start monitoring the file/folder.
            StartMonitoring();
        }

        /// <summary>
        /// Gets or sets the current status message of the file monitoring process.
        /// This message is updated when files are created or deleted.
        /// </summary>
        public string MessageStatus
        {
            //Returns the current message status
            get => _messageStatus;
            set
            {
                //updates the message status and notifies the UI about the status change.
                _messageStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Starts monitoring the specified folder for file creation and deletion events.
        /// </summary>
        private void StartMonitoring()
        {
            //Folder path to monitor
            string folderPath = @"C:\Users\harik\Downloads";

            _fileWatcher = new FileSystemWatcher
            {
                Path = folderPath,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.*"
            };

            _fileWatcher.Created += OnFileCreated;
            _fileWatcher.Deleted += OnFileDeleted;
            _fileWatcher.EnableRaisingEvents = true;

            MessageStatus = $"Monitoring folder: {folderPath}";

            _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Event handler for the Created event of the FileSystemWatcher.
        /// Add the created file path to a list and triggers the timer for processing.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A FileSystemEventArgs thta contains the event data.</param>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            lock (_createdFiles)
            {
                _createdFiles.Add(e.FullPath);
            }

            _timer.Change(1000, Timeout.Infinite); 
        }

        /// <summary>
        /// Event handler for the Deleted event of the FileSystemWatcher.
        /// Adds the deleted file path to a list and triggers the timer for processing.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A FileSystemEventArgs that contains the event data.</param>
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            lock (_deletedFiles)
            {
                _deletedFiles.Add(e.FullPath);
            }
            _timer.Change(1000, Timeout.Infinite);
        }

        /// <summary>
        /// Timer callback method that processes the lists of created and deleted files
        /// and updates the MessageStatus property with the appropriate messages.
        /// </summary>
        /// <param name="state">An object containing information about the timer event.</param>
        private async void OnTimerElapsed(object state)
        {
            List<string> filesToProcess;

            lock (_createdFiles)
            {
                filesToProcess = new List<string>(_createdFiles);
                _createdFiles.Clear();
            }

            List<string> deletedFilesToProcess;

            lock (_deletedFiles)
            {
                deletedFilesToProcess = new List<string>(_deletedFiles);
                _deletedFiles.Clear();
            }


            StringBuilder message = new StringBuilder();

            if (filesToProcess.Any())
            {
                //Sync to cloud
                foreach (var file in filesToProcess) { 
                    string fileName = Path.GetFileName(file);
                    await _cloudSyncService.UploadFileToCloud(file, fileName);
                }
                string fileList = string.Join(", ", filesToProcess.Select(Path.GetFileName));
                message.AppendLine($"Files created: {fileList}");
            }

            if (deletedFilesToProcess.Any())
            {
                //Sync to cloud
                foreach (var file in deletedFilesToProcess)
                {
                    string fileName = Path.GetFileName(file);
                    await _cloudSyncService.DeleteFileFromCloud(fileName);
                }
                string deletedFileList = string.Join(", ", deletedFilesToProcess.Select(Path.GetFileName));
                message.AppendLine($"Files removed: {deletedFileList}");
            }
            if (message.Length > 0)
            {
                string v = message.ToString();
                MessageStatus = v;
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}