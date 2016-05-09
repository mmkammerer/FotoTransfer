namespace FotoTransfer
{
    using System.Globalization;
    using System.IO;
    using System.Windows.Controls;

    class PathValidationRule : ValidationRule
    {
        public string PathDescription { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = string.Empty;
            if (value != null)
            {
                input = value.ToString();
            }

            if (string.IsNullOrEmpty(input))
            {
                return new ValidationResult(false, this.PathDescription + " ist nicht angegeben");
            }

            if (!Directory.Exists(input))
            {
                return new ValidationResult(false, this.PathDescription + " existiert nicht");
            }

            return new ValidationResult(true, null);
        }
    }
}
