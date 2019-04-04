namespace Search.IndexStrategy
{
    using System;
    using System.Collections.Generic;
    using System.Text;


    /// <summary>
    /// 
    /// </summary>
    public class PrefixIndexStrategy : IIndexStrategy
    {
        public IList<string> ExpandToken(string token)
        {
            var expandedTokens = new List<string>();
            var builder = new StringBuilder();

            foreach(char c in token)
            {
                builder.Append(c);
                expandedTokens.Add(builder.ToString());
            }

            return expandedTokens;
        }
    }
}