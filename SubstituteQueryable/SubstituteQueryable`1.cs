﻿namespace SubstituteQueryable
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using Remotion.Linq;

	public class SubstituteQueryable<T> : QueryableBase<T>
	{
		public IList<T> Deletes { get { return (Provider as SubstituteQueryProvider<T>)?.Deletes; } }
		public IList<object> Inserts { get { return (Provider as SubstituteQueryProvider<T>)?.Inserts; } }
		public IList<T> Updates { get { return (Provider as SubstituteQueryProvider<T>)?.Updates; } }

		public SubstituteQueryable(params T[] items) : this((items ?? Enumerable.Empty<T>()).AsQueryable())
		{
		}

		public SubstituteQueryable(IQueryable<T> items) : base(new SubstituteQueryProvider<T>(items))
		{
		}

		internal SubstituteQueryable(IQueryProvider provider, Expression expression) : base(provider, expression)
		{
		}
	}
}
