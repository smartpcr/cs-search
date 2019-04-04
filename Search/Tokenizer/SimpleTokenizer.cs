namespace Search.Tokenizer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// 
    /// </summary>
    public class SimpleTokenizer : ITokenizer
    {
        private static Regex spliter = new Regex("[^a-zA-Z0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IList<string> Tokenize(string text)
        {
            var tokens = spliter.Split(text).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
            return tokens;
        }
    }
}