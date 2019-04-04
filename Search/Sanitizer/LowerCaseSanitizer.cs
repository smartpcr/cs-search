namespace Search.Sanitizer
{
    /// <summary>
    /// Sanitizes text by converting to a locale-friendly lower-case version and triming leading and trailing whitespace.
    /// </summary>
    public class LowerCaseSanitizer : ISanitizer
    {
        public string Sanitize(string text)
        {
			return string.IsNullOrWhiteSpace(text) ? string.Empty : text.ToLowerInvariant().Trim();
        }
    }
}