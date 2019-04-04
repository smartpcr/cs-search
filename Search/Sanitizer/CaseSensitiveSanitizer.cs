namespace Search.Sanitizer
{
    /// <summary>
    /// Enforces case-sensitive text matches.
    /// </summary>
    public class CaseSensitiveSanitizer : ISanitizer
    {
        public string Sanitize(string text)
        {
            return !string.IsNullOrWhiteSpace(text) ? text.Trim() : string.Empty;
        }
    }
}