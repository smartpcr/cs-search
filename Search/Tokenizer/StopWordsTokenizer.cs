namespace Search.Tokenizer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json.Linq;


    /// <summary>
    /// Stop words are very common (e.g. "a", "and", "the") and are often not semantically meaningful in the context of a
    /// search. This tokenizer removes stop words from a set of tokens before passing the remaining tokens along for
    /// indexing or searching purposes.
    /// </summary>
    public class StopWordsTokenizer : ITokenizer
    {
        private readonly ITokenizer _tokenizer;
        private readonly IList<string> _stopWords;

        public StopWordsTokenizer(ITokenizer decoratedTokenizer)
        {
            _tokenizer = decoratedTokenizer;
            _stopWords = LoadStopWordsFromJsonFile("StopWords");
        }

        public IList<string> Tokenize(string text)
        {
            return _tokenizer.Tokenize(text).Where(t => !_stopWords.Contains(t.ToLower())).ToList();
        }

        private IList<string> LoadStopWordsFromJsonFile(string jsonFileName)
        {
            using (var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(jsonFileName)))
            {
                var json = reader.ReadToEnd();
                return JArray.Parse(json).Select(t => t.Value<string>()).ToList();
            }
        }
    }
}