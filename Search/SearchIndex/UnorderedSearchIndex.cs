namespace Search.SearchIndex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json.Linq;


    /// <summary>
    /// Search index capable of returning results matching a set of tokens but without any meaningful rank or order.
    /// </summary>
    public class UnorderedSearchIndex : ISearchIndex
    {
        private Dictionary<string, Dictionary<string, JToken>> _tokenToUidToDocumentMap;

        public UnorderedSearchIndex()
        {
            _tokenToUidToDocumentMap = new Dictionary<string, Dictionary<string, JToken>>();
        }

		public IList<string> Tokens
		{
			get
			{
				return _tokenToUidToDocumentMap.Keys.ToList();
			}
		}

		public void IndexDocument(string token, string uid, JToken document)
        {
            if (!_tokenToUidToDocumentMap.ContainsKey(token))
            {
                _tokenToUidToDocumentMap.Add(token, new Dictionary<string, JToken>());
            }

            _tokenToUidToDocumentMap[token].Add(uid, document);
        }

        public JArray Search(IList<string> tokens, JArray corpus)
        {
            var intersectingDocumentMap = new Dictionary<string, JToken>();
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (!_tokenToUidToDocumentMap.ContainsKey(token))
                {
                    return new JArray();
                }

                var documentMap = _tokenToUidToDocumentMap[token];
                if (i == 0)
                {
                    var keys = documentMap.Keys.ToList();
                    for(var j = 0; j < keys.Count;j++)
                    {
                        var uid = keys[j];
                        intersectingDocumentMap[uid] = documentMap[uid];
                    }
                }
                else
                {
                    var keys = intersectingDocumentMap.Keys.ToList();
                    for(var j = 0; j < keys.Count;j++)
                    {
                        var uid = keys[j];
                        if (!documentMap.ContainsKey(uid))
                        {
                            intersectingDocumentMap.Remove(uid);
                        }
                    }
                }
            }

            var uids = intersectingDocumentMap.Keys.ToList();
            var documents = new JArray();
            foreach(var uid in uids)
            {
                documents.Add(intersectingDocumentMap[uid]);
            }

            return documents;
        }
    }
}