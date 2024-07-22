using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Library.Ngram.Model;
using Nest;

namespace Library.Ngram
{
    public class ElasticSearchService : IElasticsearchServices
    {
        private readonly ElasticClient _client;

        public ElasticSearchService(IElasticsearchClientProvider clientProvider)
        {
            Console.WriteLine("minta client yg udh ada ke provider");
            _client = clientProvider.GetClient();
        }

        public void CreateIndex()
        {
            var indexExistsResponse = _client.Indices.Exists("my_product_ngram");

            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = _client.Indices.Create("my_product_ngram", c => c
                    .Settings(s => s
                        .Analysis(a => a
                            .Tokenizers(t => t
                                .NGram("3gram_tokenizer", ng => ng
                                    .MinGram(3)
                                    .MaxGram(3)
                                    .TokenChars(TokenChar.Letter, TokenChar.Digit)
                                )
                            )
                            .TokenFilters(tf => tf
                                .Lowercase("lowercase")
                            )
                            .Analyzers(an => an
                                .Custom("3gram_analyzer", ca => ca
                                    .Tokenizer("3gram_tokenizer")
                                    .Filters("lowercase")
                                )
                            )
                        )
                    )
                    .Map<DocumentModel>(m => m
                        .Properties(p => p
                            .Text(t => t
                                .Name(n => n.Content)
                                .Analyzer("3gram_analyzer")
                            )
                        )
                    )
                );
            }
        }

        public void IndexDocument<T>(T document) where T : class
        {
            _client.IndexDocument(document);
        }

        public T GetDocumentById<T>(string id) where T : class
        {
            var response = _client.Get(DocumentPath<T>.Id(id), g => g.Index("my_product_ngram"));

            if (!response.IsValid)
            {
                throw new ApplicationException($"Failed to get document from Elasticsearch: {response.DebugInformation}");
            }

            return response.Source;
        }

        public List<T> GetAllDocuments<T>() where T : class
        {
            var searchResponse = _client.Search<T>(s => s
                .Index("my_product_ngram")
                .Query(q => q.MatchAll())
                .Size(1000) // Adjust the size according to your needs
            );

            if (!searchResponse.IsValid)
            {
                throw new ApplicationException($"Failed to search documents: {searchResponse.ServerError.Error.Reason}");
            }

            return searchResponse.Documents.ToList();
        }

        public void UpdateDocument<T>(string id, T updatedDocument) where T : class
        {
            var response = _client.Update<T>(id, u => u
                .Index("my_product_ngram")
                .Doc(updatedDocument)
                .RetryOnConflict(3)
                .Refresh(Refresh.True)
            );

            if (!response.IsValid)
            {
                throw new ApplicationException($"Failed to update document in Elasticsearch: {response.ServerError}");
            }
        }

        public void DeleteDocument(string id)
        {
            var response = _client.Delete<DocumentModel>(id, d => d.Index("documents"));
            if (!response.IsValid)
            {
                throw new ApplicationException($"Failed to delete document in Elasticsearch: {response.ServerError.Error.Reason}");
            }
        }

        public IEnumerable<T> SearchWithNGram<T>(string searchTerm, string field) where T : class
        {
            var searchResponse = _client.Search<T>(s => s
                .Index("my_product_ngram")
                .Query(q => q
                    .Match(m => m
                        .Field(field)
                        .Query(searchTerm)
                        .Analyzer("3gram_analyzer")
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new ApplicationException($"Failed to search documents: {searchResponse.ServerError.Error.Reason}");
            }

            return searchResponse.Documents;
        }

        public IEnumerable<T> SearchWithPagination<T>(string searchTerm, string field, int pageNumber, int pageSize) where T : class
        {
            int from = (pageNumber - 1) * pageSize;
            var searchResponse = _client.Search<T>(s => s
                .Index("my_product_ngram")
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .Match(m => m
                        .Field(field)
                        .Query(searchTerm)
                        .Analyzer("3gram_analyzer")
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new ApplicationException($"Failed to search documents: {searchResponse.ServerError.Error.Reason}");
            }

            return searchResponse.Documents;
        }
    }
}
