namespace FotoTransfer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using ExifLib;
    using System.Diagnostics;
    using JetBrains.Annotations;

    /// <summary>
    /// A class that can find and copy JPG files based on the date when the photo was taken
    /// </summary>
    public class ExifFileHandler
    {
        private List<ExifFile> files;

        private TransferProgress transferProgress = new TransferProgress();

        /// <summary>
        /// Copies a single file to the target path.
        /// </summary>
        /// <param name="sourceFile">The source path and file name.</param>
        /// <param name="targetPath">The target path.</param>
        /// <param name="keepOriginalFileName">If true, the original file name is used, otherwise the file name is replaced by a time stamp.</param>
        /// <returns>true if the file was copied, otherwise false.</returns>
        public bool CopySingleFile([NotNull]string sourceFile, [NotNull]string targetPath, bool keepOriginalFileName)
        {
            if (!File.Exists(sourceFile))
            {
                return false;
            }

            var exifFile = new ExifFile(sourceFile);
            if (!exifFile.ReadMetaDataExifLib())
            {
                return false;
            }

            return this.CopyExifFile(exifFile, targetPath, keepOriginalFileName);
        }

        /// <summary>
        /// Finds the files in the specified date range in sourcePath and copies them to targetPath
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="targetPath">The target path.</param>
        /// <param name="startTime">The start date and time.</param>
        /// <param name="endTime">The end date and time.</param>
        /// <param name="keepOriginalFileName">If true, the original file name is used, otherwise the file name is replaced by a time stamp.</param>
        /// <param name="progress">The progress handler.</param>
        public void FindAndCopyFiles([NotNull]string sourcePath, [NotNull]string targetPath, DateTime startTime, DateTime endTime, bool keepOriginalFileName, IProgress<TransferProgress> progress)
        {
            int numFiles = this.FindFiles(sourcePath, startTime, endTime, progress);
            if (numFiles == 0)
            {
                this.transferProgress.ProgressInfo = "Es wurden keine Fotos gefunden";
                this.transferProgress.State = TransferState.Done;
                progress.Report(this.transferProgress);
                return;
            }

            this.CopyFiles(targetPath, keepOriginalFileName, progress);
        }

        /// <summary>
        /// Searches for files in the specified date range in source path.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="startTime">The start date and time.</param>
        /// <param name="endTime">The end date and time.</param>
        /// <param name="progress">The progress handler.</param>
        /// <returns>The number of files found.</returns>
        private int FindFiles([NotNull]string sourcePath, DateTime startTime, DateTime endTime, IProgress<TransferProgress> progress)
        {
            DateTime minTime = startTime.Date;
            DateTime maxTime = endTime.Date + TimeSpan.FromDays(1);

            this.files = new List<ExifFile>();
            int filesFound = 0;

            var allFiles = Directory.GetFiles(sourcePath, "*.jpg", SearchOption.AllDirectories);
            var totalFiles = allFiles.Length;
            int filesSearched = 0;
            double lastPercentageReported = 0.0;
            bool updateReport;

            this.transferProgress.State = TransferState.Searching;
            this.transferProgress.ProgressInfo = string.Format("Durchsuche {0} Fotos ...", totalFiles);
            progress.Report(this.transferProgress);

            foreach (string file in allFiles)
            {
                updateReport = false;
                ExifFile exifFile = new ExifFile(file);
                if (exifFile.ReadMetaDataExifLib())
                {
                    if (exifFile.Taken >= minTime && exifFile.Taken < maxTime)
                    {
                        this.files.Add(exifFile);
                        filesFound++;
                        this.transferProgress.ProgressInfo = string.Format("{0} Foto{1} gefunden ...", filesFound, filesFound == 1 ? string.Empty : "s");
                        updateReport = true;
                    }
                }

                filesSearched++;
                this.transferProgress.Percentage = filesSearched * 100.0 / totalFiles;
                if (this.transferProgress.Percentage - lastPercentageReported >= 1.0)
                {
                    updateReport = true;
                }

                if (updateReport)
                {
                    progress.Report(this.transferProgress);
                    lastPercentageReported = Math.Floor(this.transferProgress.Percentage);
                }
            }

            return filesFound;
        }

        /// <summary>
        /// Copies the files that have been searched before to the target path using the new file names.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        /// <param name="keepOriginalFileName">If true, the original file name is used, otherwise the file name is replaced by a time stamp.</param>
        /// <param name="progress">The progress handler.</param>
        private void CopyFiles(string targetPath, bool keepOriginalFileName, IProgress<TransferProgress> progress)
        {
            int totalFiles = this.files.Count;
            int filesCopied = 0;

            this.transferProgress.State = TransferState.Copying;

            foreach (var exifFile in this.files)
            {
                this.CopyExifFile(exifFile, targetPath, keepOriginalFileName);

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

        /// <summary>
        /// Copies a single exif File to the target path.
        /// </summary>
        /// <param name="exifFile">The exif source file.</param>
        /// <param name="targetPath">The target path.</param>
        /// <param name="keepOriginalFileName">If true, the original file name is used, otherwise the file name is replaced by a time stamp.</param>
        /// <returns>true if the file was copied, otherwise false.</returns>
        private bool CopyExifFile(ExifFile exifFile, string targetPath, bool keepOriginalFileName)
        {
            string sourceFileName = exifFile.OriginalFileName;
            string targetFileName;
            if (keepOriginalFileName)
            {
                targetFileName = Path.Combine(targetPath, Path.GetFileName(sourceFileName));
            }
            else
            {
                targetFileName = Path.Combine(targetPath, exifFile.NewFileName);
            }

            if (!File.Exists(targetFileName))
            {
                File.Copy(sourceFileName, targetFileName);
                return true;
            }

            return false;
        }

    }
}
