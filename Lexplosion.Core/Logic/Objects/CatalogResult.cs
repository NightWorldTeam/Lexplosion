﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Logic.Objects
{
	public class CatalogResult<T> : IReadOnlyCollection<T>
	{
		public List<T> Collection { get; set; } = [];
		public int TotalCount { get; set; }

		public int Count => Collection.Count;


		public CatalogResult()
		{

		}


		public CatalogResult(List<T> collection, int pageCount)
		{
			Collection = collection;
			TotalCount = pageCount;
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
