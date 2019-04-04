namespace Search.IndexStrategy
{
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public interface IIndexStrategy
    {
        IList<string> ExpandToken(string token);
    }
}
