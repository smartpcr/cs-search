namespace Search.Sanitizer
{
    /// <summary>
    /// A sanitizer helps convert searchable field text and user query text to a format that can be easily compared. Among
    /// other things, this often involves operations like trimming leading and trailing whitespace.
    /// </summary>
    public interface ISanitizer
    {
        string Sanitize(string text);
    }
}