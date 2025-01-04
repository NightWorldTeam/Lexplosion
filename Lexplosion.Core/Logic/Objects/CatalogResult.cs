using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Core.Logic.Objects
{
    public class CatalogResult<T> : IReadOnlyCollection<T>
    {
        public List<T> Collection { get; set; } = [];
        public int PageCount { get; set; }

        public int Count => Collection.Count;


        public CatalogResult()
        {
            
        }


        public CatalogResult(List<T> collection, int pageCount)
        {
            Collection = collection;
            PageCount = pageCount;
        }


        public IEnumerator<T> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Collection.GetEnumerator();
        }
    }
}
