namespace Search.IndexStrategy
{
    using System;
    using System.Collections.Generic;
    using System.Text;


    /// <summary>
    /// 
    /// </summary>
    public class AllSubstringsIndexStrategy : IIndexStrategy
    {
        public IList<string> ExpandToken(string token)
        {
            var expandedTokens = new List<string>();
            var builder = new StringBuilder();

            for(var i = 0; i < token.Length; i++)
            {
                builder = new StringBuilder();
                for(var j = i; j<token.Length; j++)
                {
                    builder.Append(token[j]);
                    expandedTokens.Add(builder.ToString());
                }
            }

            return expandedTokens;
        }
    }
}