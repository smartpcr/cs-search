namespace Search.Tokenizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Stemming is the process of reducing search tokens to their root (or stem) so that searches for different forms of a
    /// word will match. For example "search", "searching" and "searched" are all reduced to the stem "search".
    ///
    /// This stemming tokenizer converts tokens (words) to their stem forms before returning them. It requires an
    /// external stemming function to be provided; for this purpose I recommend the NPM 'porter-stemmer' library.
    ///
    /// For more information see http : //tartarus.org/~martin/PorterStemmer/
    /// </summary>
    public class StemmingTokenizer : ITokenizer
    {
        private readonly Func<string, string> _stemmingFunction;
        private readonly ITokenizer _tokenizer;

        public StemmingTokenizer(Func<string, string> stemmingFunction, ITokenizer decoratedTokenizer)
        {
            _stemmingFunction = stemmingFunction;
            this._tokenizer = decoratedTokenizer;
        }

        public IList<string> Tokenize(string text)
        {
            return _tokenizer.Tokenize(text).Select(a => _stemmingFunction(a)).ToList();
        }
    }
}