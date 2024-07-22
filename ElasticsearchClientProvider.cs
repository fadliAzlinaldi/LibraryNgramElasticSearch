using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Ngram
{
    public class ElasticsearchClientProvider : IElasticsearchClientProvider
    {
        private readonly string _uri;
        public readonly ElasticClient _client;

        public ElasticsearchClientProvider(string uri)
        {
            _uri = uri;
            var settings = new ConnectionSettings(new Uri(_uri))
                .DefaultIndex("my_product_ngram");

            Console.WriteLine("buka koneksi client");
            _client = new ElasticClient(settings);
        }
        public ElasticClient GetClient()
        {
            Console.WriteLine("manggil client yg udh ada");
            return _client;
        }
    }
}
