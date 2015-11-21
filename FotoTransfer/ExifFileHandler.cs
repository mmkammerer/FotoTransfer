
// inserts a small sleep after each file to test the progress bar and info output
// #define SLEEP_FOR_DEBUG

namespace FotoTransfer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using ExifLib;

    /// <summary>
    /// A class that can find and copy JPG files based on the date when the photo was taken
    /// </summary>
    public class ExifFileHandler
    {
        private List<ExifFile> files;

        private TransferProgress transferProgress = new TransferProgress();

        /// <summary>
        /// Finds the files in the specified date range in sourcePath and copies them to targetPath
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="targetPath">The target path.</param>
        /// <param name="startTime">The start date and time.</param>
        /// <param name="endTime">The end date and time.</param>
        /// <param name="progress">The progress handler.</param>
        public void FindAndCopyFiles(string sourcePath, string targetPath, DateTime startTime, DateTime endTime, IProgress<TransferProgress> progress)
        {
            int numFiles = this.FindFiles(sourcePath, startTime, endTime, progress);
            if (numFiles == 0)
            {
                this.transferProgress.ProgressInfo = "Es wurden keine Fotos gefunden";
                this.transferProgress.State = TransferState.Done;
                progress.Report(this.transferProgress);
                return;
            }

            this.CopyFiles(targetPath, progress);
        }

        /// <summary>
        /// Searches for files in the specified date range in source path.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="startTime">The start date and time.</param>
        /// <param name="endTime">The end date and time.</param>
        /// <param name="progress">The progress handler.</param>
        /// <returns>The number of files found.</returns>
        private int FindFiles(string sourcePath, DateTime startTime, DateTime endTime, IProgress<TransferProgress> progress)
        {
            DateTime minTime = startTime.Date;
            DateTime maxTime = endTime.Date + TimeSpan.FromDays(1);

            this.transferProgress.State = TransferState.Searching;
            this.transferProgress.ProgressInfo = "Suche Fotos ...";
            progress.Report(this.transferProgress);

            this.files = new List<ExifFile>();
            int filesFound = 0;

            foreach (string file in Directory.EnumerateFiles(sourcePath, "*.jpg", SearchOption.AllDirectories))
            {

#if SLEEP_FOR_DEBUG
                System.Threading.Thread.Sleep(100);
#endif

                ExifFile exifFile = new ExifFile(file);
                if (exifFile.ReadMetaDataExifLib())
                {
                    if (exifFile.Taken >= minTime && exifFile.Taken < maxTime)
                    {
                        files.Add(exifFile);
                        filesFound++;
                        this.transferProgress.ProgressInfo = string.Format("{0} Foto{1} gefunden ...", filesFound, filesFound == 1 ? string.Empty : "s");
                        progress.Report(this.transferProgress);
                    }
                }
            }

            return filesFound;
        }

        /// <summary>
        /// Copies the files that have been searched before to the target path using the new file names.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        /// <param name="progress">The progress handler.</param>
        private void CopyFiles(string targetPath, IProgress<TransferProgress> progress)
        {
            int totalFiles = this.files.Count;
            int filesCopied = 0;

            this.transferProgress.State = TransferState.Copying;

            foreach (var exifFile in this.files)
            {
#if SLEEP_FOR_DEBUG
                System.Threading.Thread.Sleep(100);
#endif

                string sourceFileName = exifFile.OriginalFileName;
                string targetFileName = Path.Combine(targetPath, exifFile.NewFileName);
                if (!File.Exists(targetFileName))
                {
                    File.Copy(sourceFileName, targetFileName);
                }

                filesCopied++;
                this.transferProgress.ProgressInfo = string.Format("{0} von {1} Fotos kopiert ...", filesCopied, totalFiles);
                this.transferProgress.Percentage = filesCopied * 100.0 / totalFiles;
                progress.Report(this.transferProgress);
            }

            this.transferProgress.Percentage = 0.0;
            this.transferProgress.ProgressInfo = string.Format("{0} Fotos wurden kopiert.", filesCopied);
            this.transferProgress.State = TransferState.Done;
            progress.Report(this.transferProgress);
        }
    }
}
