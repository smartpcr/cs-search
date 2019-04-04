namespace Search.SearchIndex
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;


    /// <summary>
    /// A search index stores documents in such a way as to enable quick lookup against one or more tokens.
    /// </summary>
    public interface ISearchIndex
    {
		IList<string> Tokens { get; }

        /// <summary>
        /// Track the specified document and token association.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <param name="document"></param>
        void IndexDocument(string token, string uid, JToken document);

        /// <summary>
        /// Return all documents that match the specified tokens
        /// </summary>
        /// <param name="tokens">Tokenized query (eg "the boy" query becomes ["the", "boy"] tokens)</param>
        /// <param name="corpus">All document in search corpus</param>
        /// <returns>Array of matching documents</returns>
        JArray Search(IList<string> tokens, JArray corpus);
    }
}