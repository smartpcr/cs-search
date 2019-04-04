Feature: SearchIndex

Background:
	Given uid field "Id"
	And A set of documents
    | id | title                                             |
    | a  | this document is about node.                      |
    | b  | this document is about ruby.                      |
    | c  | this document is about ruby and node.             |
    | d  | this document is about node. It has node examples |
	And searchable fields 
    | field |
    | Title |

@index @tfidf
Scenario: Idf
    When I index the documents
	Then I should get the following IDF values
    | token    | numDocsContainingToken |
    | and      | 1                      |
    | document | 4                      |
    | node     | 3                      |
    | foobar   | 0                      |
    | ruby     | 2                      |

@index @tfidf
Scenario: TfIdf single token
    When I index the documents
	Then I should get the following TfIdf values
    | tokens         | documentId | numDocsWithToken | tokenCountInDoc |
    | node           | a          | 3                | 1               |
    | node           | d          | 3                | 2               |
    | node           | b          | 3                | 0               |

@index @tfidf
Scenario: TfIdf multi-word token
    When I index the documents
	Then I should get the following TfIdf values
    | tokens         | documentId | numDocsWithToken | tokenCountInDoc |
    | has node       | b          | 0                | 0               |
	| has node       | d          | 0                | 0               |

@index @tfidf
Scenario: TfIdf no document
    When I index the documents
	Then I should get the following TfIdf values given no document
    | tokens | tfidf |
    | foobar | 0     |

@index @tfidf
Scenario: TfIdf multiple tokens
    When I index the documents
	Then I should get the following score for document with id "d"
	"""
	[
		{
			"token": "document",
			"numDocsWithToken": 4,
			"tokenCountInDoc": 1
		},
		{
			"token": "node",
			"numDocsWithToken": 3,
			"tokenCountInDoc": 2
		}
	]
	"""

@search @tfidf
Scenario: should get ordered search results
	When I search the documents with query "node"
	Then I should get the following search results
	| docId |
	| d     |
	| a     |
	| c     |

@search @tfidf
Scenario: should give documents containing words with a lower IDF a higher relative ranking
	Given add the following extra documents
	| id | title                                                                                       |
	| e  | foo bar foo bar baz baz baz baz                                                             |
	| f  | foo bar foo foo baz baz baz baz                                                             |
	| g  | foo bar baz bar baz baz baz baz                                                             |
	| h  | foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo |
	| i  | foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo |
	| j  | foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo |
	| k  | foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo |
	| l  | foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo |
	| m  | foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo |
	| n  | foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo baz foo foo |
	When I search the documents with query "foo bar"
	Then I should get the following search results
	| docId |
	| e     |
	| f     |
	| g     |

@search @tfidf
Scenario Outline: searchable fields should support nested field path
	Given add the following complex documents
	"""
	[
		{
			"Id": "2562",
			"login": {
				"username": "Melissa Smith",
				"domain": "google.com"
			}
		},
		{
			"Id": "54213",
			"login": {
				"username": "John Smith",
				"domain": "microsoft.com"
			}
		}
	]
	"""
	And add extra searchable fields 
    | field          |
    | login.username |
    | login.domain   |
	When I search the documents with query "<query>"
	Then I should get the following <searchResults> in output
	
	Examples:
	| query   | searchResults |
	| Melissa | "2562"        |
	| John    | "54213"       |
	| Smith   | "2562,54213"  |

@search @partial-search
Scenario Outline: should be able to get results with partial match
	When I search the documents with query "<query>"
	Then I should get the following <searchResults> in output

	Examples:
	| query  | searchResults |
	| ru     | "b","c"       |
	| ab nod | "d,a,c"       |

@search @substring
Scenario Outline: should be able to get results with partial contains match
	Given using substring index strategy
	When I search the documents with query "<query>"
	Then I should get the following <searchResults> in output

	Examples:
	| query   | searchResults |
	| ub      | "b","c"       |
	| out nod | "d,a,c"       |