namespace Search.IndexStrategy
{
    using System;
    using System.Collections.Generic;
    using System.Text;


    /// <summary>
    /// 
    /// </summary>
    public class ExactWordIndexStrategy : IIndexStrategy
    {
        public IList<string> ExpandToken(string token)
        {
            return !string.IsNullOrWhiteSpace(token) ? new List<string> { token } : new List<string>();
        }
    }
}