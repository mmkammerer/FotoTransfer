namespace FotoTransfer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using ExifLib;
    using System.Diagnostics;

    /// <summary>
    /// Class for a file with EXIF information
    /// </summary>
    public class ExifFile
    {
        private static readonly Dictionary<string, string> cameraModelDictionary = new Dictionary<string,string>()
        {
            { "Canon PowerShot SX120 IS", "PS"}, // Micha
            { "Canon EOS 450D", "FH" },          // Fabian
            { "Canon IXUS 130", "X3"},           // Christiane, alte Kamera
            { "Canon IXUS 170", "X7"},           // Christiane
            { "XT1039", "MG"},                   // MOTO G, Micha und Christiane
            { "W890i", "W8"},                    // Sony Ericsson Handy Micha
            { "iPhone 5", "i5" },                // iPhone Fabian, Theresa
            { "iPhone 6", "i6" },                // iPhone Theresa
            { "SM-G800F", "s5m" }                // Samsung S5 mini Christiane
        };

        /// <summary>
        /// Creates a new instance of the <see cref="ExifFile"/> class.
        /// </summary>
        /// <param name="fileName">The full path and file name.</param>
        public ExifFile(string fileName)
        {
            this.OriginalFileName = fileName;
        }

        /// <summary>
        /// The date and time when the picture was taken
        /// </summary>
        public DateTime Taken { get; private set; }

        /// <summary>
        /// The original path and file name
        /// </summary>
        public string OriginalFileName { get; private set; }

        /// <summary>
        /// The new file name (without path) consisting of a time stamp and a camera model appendix
        /// </summary>
        public string NewFileName { get; private set; }

        /// <summary>
        /// The new file name (without path) consisting of the old name plus a camera model appendix
        /// </summary>
        public string NearlyOriginalFileName { get; private set; }

        /// <summary>
        /// Reads the meta data using ExifLib (much more faster!)
        /// </summary>
        /// <returns>true if date and camera model could be read from the file, otherwise false</returns>
        public bool ReadMetaDataExifLib()
        {
            ExifReader exifReader = null;

            try
            {
                exifReader = new ExifReader(this.OriginalFileName);

                DateTime taken;
                if (!exifReader.GetTagValue<DateTime>(ExifTags.DateTimeOriginal, out taken))
                {
                    // If date/time taken is not contained in the EXIF information use the file modified date (LastWriteTime).
                    // This is usually the time the file was saved in the camera
                    FileInfo fi = new FileInfo(this.OriginalFileName);
                    taken = fi.LastWriteTime;
                }

                this.Taken = taken;
                string dateTakenIso = this.Taken.ToString("yyyyMMdd_HHmmss");

                string model = string.Empty;
                string suffix = string.Empty;
                if (exifReader.GetTagValue<string>(ExifTags.Model, out model))
                {
                    string entry = cameraModelDictionary.FirstOrDefault(e => model.IndexOf(e.Key, StringComparison.OrdinalIgnoreCase) >= 0).Value;
                    suffix = string.IsNullOrEmpty(entry) ? string.Empty : "_" + entry;
                }

                this.NewFileName = string.Format("IMG_{0}{1}.jpg", dateTakenIso, suffix);
                this.NearlyOriginalFileName = string.Format("{0}{1}.jpg", Path.GetFileNameWithoutExtension(this.OriginalFileName), suffix);
                
                return true;
            }
            catch (ExifLibException e)
            {
                // e.g. if no EXIF information is present at all
                Debug.WriteLine("{0} in Datei {1}: {2}", e.GetType().Name, this.OriginalFileName, e.Message);
                return false;
            }
            finally
            {
                if (exifReader != null)
                {
                    exifReader.Dispose();
                }
            }
        }
    }
}
