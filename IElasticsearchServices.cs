using Library.Ngram.Model;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Ngram
{
    public interface IElasticsearchServices
    {
        void CreateIndex();
        void IndexDocument<T>(T document) where T : class;
        void UpdateDocument<T>(string id, T updatedDocument) where T : class;
        void DeleteDocument(string id);
        T GetDocumentById<T>(string id) where T : class;
        List<T> GetAllDocuments<T>() where T : class;
        IEnumerable<T> SearchWithNGram<T>(string searchTerm, string field) where T : class;
        IEnumerable<T> SearchWithPagination<T>(string searchTerm, string field, int pageNumber, int pageSize) where T : class;

    }
}
