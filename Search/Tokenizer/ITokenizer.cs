namespace Search.Tokenizer
{
    using System.Collections.Generic;

    /// <summary>
    /// A tokenizer converts a string of text (e.g. "the boy") to a set of tokens (e.g. "the", "boy"). These tokens are used
    /// for indexing and searching purposes.
    /// </summary>
    public interface ITokenizer
    {
        IList<string> Tokenize(string text);
    }
}