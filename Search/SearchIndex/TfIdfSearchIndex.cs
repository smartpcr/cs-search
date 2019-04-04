namespace Search.SearchIndex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    public class TfIdfTokenMap
    {
        public Dictionary<string, TfIdfTokenMetadata> Map { get; set; }

        public TfIdfTokenMap()
        {
            Map = new Dictionary<string, TfIdfTokenMetadata>();
        }
    }

    public class TfIdfTokenMetadata
    {
        public int NumDocumentOccurrences { get; set; }
        public int TotalNumOccurrences { get; set; }
        public TfIdfUidMap UidMap { get; set; }
    }

    public class TfIdfUidMap
    {
        public Dictionary<string, TfIdfUidMetadata> Map { get; set; }

        public TfIdfUidMap()
        {
            Map = new Dictionary<string, TfIdfUidMetadata>();
        }
    }

    public class TfIdfUidMetadata
    {
        public JToken Document { get; set; }
        public int NumTokenOccurrences { get; set; }
    }

    /// <summary>
    /// Search index capable of returning results matching a set of tokens and ranked according to TF-IDF.
    /// </summary>
    public class TfIdfSearchIndex : ISearchIndex
    {
        private readonly string _uidFieldName;
        private Dictionary<string, double> _tokenToIdfCache;
        private TfIdfTokenMap _tokenMap;

        public TfIdfSearchIndex(string uidFieldName)
        {
            _uidFieldName = uidFieldName;
            _tokenToIdfCache = new Dictionary<string, double>();
            _tokenMap = new TfIdfTokenMap();
        }

		public IList<string> Tokens
		{
			get
			{
				return _tokenMap.Map.Keys.ToList();
			}
		}

        public void IndexDocument(string token, string uid, JToken document)
        {
            // New index invalidates previous IDF caches
            _tokenToIdfCache = new Dictionary<string, double>();
            TfIdfTokenMetadata tokenDatum;
            if (!_tokenMap.Map.ContainsKey(token))
            {
                tokenDatum = new TfIdfTokenMetadata
                {
                    NumDocumentOccurrences = 0,
                    TotalNumOccurrences = 1,
                    UidMap = new TfIdfUidMap()
                };
                _tokenMap.Map.Add(token, tokenDatum);
            }
            else
            {
                tokenDatum = _tokenMap.Map[token];
                tokenDatum.TotalNumOccurrences++;
            }

            var uidMap = tokenDatum.UidMap;
            if (!uidMap.Map.ContainsKey(uid))
            {
				tokenDatum.NumDocumentOccurrences++;
                var uidMeta = new TfIdfUidMetadata
                {
                    Document = document,
                    NumTokenOccurrences = 1
                };
                uidMap.Map.Add(uid, uidMeta);
            }
            else
            {
                uidMap.Map[uid].NumTokenOccurrences++;
            }
        }

        public JArray Search(IList<string> tokens, JArray corpus)
        {
            var uidToDocumentMap = new Dictionary<string, JToken>();

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var tokenMetadata = _tokenMap.Map.ContainsKey(token) ? _tokenMap.Map[token] : null;
                if (tokenMetadata == null)
                {
                    return new JArray();
                }

                if (i == 0)
                {
                    var keys = tokenMetadata.UidMap.Map.Keys.ToList();
                    for (var j = 0; j < keys.Count; j++)
                    {
                        var uid = keys[j];
                        uidToDocumentMap[uid] = tokenMetadata.UidMap.Map[uid].Document;
                    }
                }
                else
                {
                    var keys = uidToDocumentMap.Keys.ToList();
                    for (var j = 0; j < keys.Count; j++)
                    {
                        var uid = keys[j];
                        if (!tokenMetadata.UidMap.Map.ContainsKey(uid))
                        {
                            uidToDocumentMap.Remove(uid);
                        }
                    }
                }
            }

            var scoredDocuments = new Dictionary<double, List<JToken>>();
            foreach (var uid in uidToDocumentMap.Keys)
            {
                var document = uidToDocumentMap[uid];
                var score = CalculateTfIdf(tokens, document, corpus);
                if (scoredDocuments.ContainsKey(score))
                {
                    scoredDocuments[score].Add(document);
                }
                else
                {
                    scoredDocuments.Add(score, new List<JToken> { document });
                }
            }

            return new JArray(scoredDocuments.OrderByDescending(p => p.Key).SelectMany(p => p.Value).ToList());
        }

        internal double CalculateTfIdf(IList<string> tokens, JToken document, JArray documents)
        {
            double score = 0;

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var inverseDocumentFrequency = CalculateIdf(token, documents);

                if (double.IsInfinity(inverseDocumentFrequency))
                {
                    inverseDocumentFrequency = 0;
                }

                var uid = document.Value<string>(_uidFieldName);
                var termFrequency = _tokenMap.Map.ContainsKey(token) && _tokenMap.Map[token].UidMap.Map.ContainsKey(uid)
                    ? _tokenMap.Map[token].UidMap.Map[uid].NumTokenOccurrences
                    : 0;
                score += termFrequency * inverseDocumentFrequency;
            }

            return score;
        }

		internal double CalculateIdf(string token, JArray documents)
        {
            if (!_tokenToIdfCache.ContainsKey(token))
            {
                var numDocumentsWithToken = _tokenMap.Map.ContainsKey(token)
					? _tokenMap.Map[token].NumDocumentOccurrences
					: 0;
                _tokenToIdfCache.Add(token, 1 + Math.Log((double)documents.Count / (1 + numDocumentsWithToken)));
            }
            return _tokenToIdfCache[token];
        }
    }
}