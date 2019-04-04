namespace Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json.Linq;
    using Search.IndexStrategy;
    using Search.Sanitizer;
    using Search.SearchIndex;
    using Search.Tokenizer;


    /// <summary>
    /// 
    /// </summary>
    public class Searcher
    {
        private bool _initialized;
		private string _uidFieldName;

		public JArray Documents { get; private set; }
        public List<string> SearchableFields { get; private set; }
        public IIndexStrategy IndexStrategy { get; private set; }
        public ISanitizer TokenSantizer { get; private set; }
        public ISearchIndex SearchIndex { get; private set; }
        public ITokenizer Tokenizer { get; private set; }
        public Searcher(string uidFieldName)
        {
            _uidFieldName = uidFieldName ?? throw new ArgumentException(nameof(uidFieldName));
            _initialized = false;

			SearchableFields = new List<string>();
			Documents = new JArray();
			IndexStrategy = new PrefixIndexStrategy();
            SearchIndex = new TfIdfSearchIndex(_uidFieldName);
            TokenSantizer = new LowerCaseSanitizer();
            Tokenizer = new SimpleTokenizer();
        }

        public Searcher WithIndexStrategy(IIndexStrategy indexStrategy)
        {
            if (_initialized)
            {
                throw new Exception("IIndexStrategy cannot be set after initialization");
            }
            IndexStrategy = indexStrategy;

            return this;
        }

        public Searcher WithSanitizer(ISanitizer sanitizer)
        {
            if (_initialized)
            {
                throw new Exception("ISanitizer cannot be set after initialization");
            }
            TokenSantizer = sanitizer;

            return this;
        }

        public Searcher WithSearchIndex(ISearchIndex searchIndex)
        {
            if (_initialized)
            {
                throw new Exception("ISearchIndex cannot be set after initialization");
            }
            SearchIndex = searchIndex;

            return this;
        }

        public Searcher WithTokenizer(ITokenizer tokenizer)
        {
            if (_initialized)
            {
                throw new Exception("ITokenizer cannot be set after initialization");
            }
            Tokenizer = tokenizer;

            return this;
        }

        public void AddIndex(params string[] fields)
        {
            SearchableFields.AddRange(fields);
            IndexDocuments(Documents, SearchableFields);
        }

        public void AddDocument(JToken document)
        {
			Documents.Add(document);
			IndexDocuments(Documents, SearchableFields);
		}

        public void SetDocuments(JArray documents)
        {
            Documents = documents;
            IndexDocuments(Documents, SearchableFields);
        }

        public JArray Search(string query)
        {
            var tokens = Tokenizer.Tokenize(TokenSantizer.Sanitize(query));
            return SearchIndex.Search(tokens, Documents);
        }

        private void IndexDocuments(JArray documents, IList<string> searchableFields)
        {
            foreach(var document in documents)
            {
                var uid = document.Value<string>(_uidFieldName);
                foreach(var searchableField in searchableFields)
                {
                    var fieldValues = GetFieldValue(document, searchableField);
                    if (fieldValues != null && fieldValues.Count > 0)
                    {
                        foreach(var fieldValue in fieldValues)
                        {
                            var fieldTokens = Tokenizer.Tokenize(TokenSantizer.Sanitize(fieldValue));
                            foreach(var fieldToken in fieldTokens)
                            {
                                var expandedTokens = IndexStrategy.ExpandToken(fieldToken);
                                foreach(var expandToken in expandedTokens)
                                {
                                    SearchIndex.IndexDocument(expandToken, uid, document);
                                }
                            }
                        }
                    }
                }
            }

            _initialized = true;
        }

        private IList<string> GetFieldValue(JToken @object, string fieldPath)
        {
            if (fieldPath.IndexOf(".")>0)
            {
                var currentFieldName = fieldPath.Substring(0, fieldPath.IndexOf("."));
				var nestedFieldPath = fieldPath.Substring(fieldPath.IndexOf(".") + 1);

				var nestedObject = @object.Value<JToken>(currentFieldName);
				if (nestedObject==null)
				{
					return null;
				}

                if (nestedObject.Type == JTokenType.Array)
                {
                    var output = new List<string>();
                    foreach(var childObj in (JArray)nestedObject)
                    {
                        output.AddRange(GetFieldValue(childObj, nestedFieldPath));
                    }
                    return output;
                }
                else
                {
                    return GetFieldValue(nestedObject, nestedFieldPath);
                }
            }

            var value = @object[fieldPath];
            if (value == null)
            {
                return null;
            }

            if (value.Type == JTokenType.Array)
            {
                var output = new List<string>();
                foreach(var token in (JArray)value)
                {
                    if (token.Type == JTokenType.String)
                    {
                        output.Add((string)token);
                    }
                }
                return output;
            }
            else if (value.Type == JTokenType.String)
            {
                return new List<string> { (string)value };
            }
            else
            {
                throw new Exception($"Expecting string type, found type {value.Type} for path ${fieldPath}");
            }
        }
    }
}