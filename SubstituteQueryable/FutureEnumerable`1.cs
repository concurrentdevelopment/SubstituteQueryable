﻿namespace SubstituteQueryable
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using NHibernate;

	internal class FutureEnumerable<T> : IFutureEnumerable<T>
	{
		private readonly IEnumerable<T> _items;

		public FutureEnumerable(IEnumerable<T> items)
		{
			_items = items;
		}

		public IEnumerable<T> GetEnumerable()
		{
			return _items;
		}

		public Task<IEnumerable<T>> GetEnumerableAsync(CancellationToken cancellationToken = default)
		{
			return Task.FromResult(_items);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
