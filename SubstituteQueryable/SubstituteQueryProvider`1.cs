namespace SubstituteQueryable
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Threading;
	using System.Threading.Tasks;
	using NHibernate;
	using NHibernate.Linq;
	using NHibernate.Type;

	// https://stackoverflow.com/questions/39338308/implementing-a-custom-queryprovider-with-in-memory-query/42710376#42710376
	// https://stackoverflow.com/questions/33286244/nhibernate-fetch-calls-fail-on-a-mocked-session#:~:text=Fetch%20is%20an%20extension%20method,You%20can%20use%20a%20wrapper.
	internal class SubstituteQueryProvider<T> : INhQueryProvider
	{
		public IList<T> Deletes { get; private set; } = new List<T>();

		public IList<object> Inserts { get; private set; } = new List<object>();

		public IList<T> Updates { get; private set; } = new List<T>();

		public IQueryable<T> Source { get; set; }

		public SubstituteQueryProvider(IQueryable<T> source)
		{
			Source = source;
		}

		public IQueryable CreateQuery(Expression expression)
		{
			var elementType = expression.Type.GetExpressionElementType();

			return (IQueryable)Activator.CreateInstance(typeof(SubstituteQueryable<>)
										.MakeGenericType(elementType), new object[] { this, expression });
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return new SubstituteQueryable<TElement>(this, expression);
		}

		public object Execute(Expression expression)
		{
			return ExecuteInMemoryQuery(expression, false);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			bool IsEnumerable = typeof(TResult).Name == "IEnumerable`1";
			return (TResult)ExecuteInMemoryQuery(expression, IsEnumerable);
		}

		public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			return Task.FromResult(Execute<TResult>(expression));
		}

		public int ExecuteDml<T1>(QueryMode queryMode, Expression expression)
		{
			int count = 0;
			if (queryMode == QueryMode.Delete)
			{
				foreach (var item in Expression.Lambda(expression).Compile().DynamicInvoke() as IEnumerable<T>)
				{
					count++;
					Deletes.Add(item);
				}
				return count;
			}

			var methodCall = expression as MethodCallExpression;
			var translation = methodCall.Arguments.Last();
			var filtered = Expression.Lambda<Func<IEnumerable<T>>>(methodCall.Arguments.First()).Compile()();

			if (queryMode == QueryMode.Insert)
			{				
				foreach (var item in filtered)
				{
					var result = Activator.CreateInstance<T1>();

					new BlockVisitor<T, T1>(item, result).Visit(translation);

					count++;

					Inserts.Add(result);
				}
				return count;
			}

			if (queryMode == QueryMode.Update)
			{
				foreach (var item in filtered)
				{
					var result = Activator.CreateInstance<T>();

					new BlockVisitor<T, T>(item, result).Visit(translation);

					count++;

					Updates.Add(result);
				}
				return count;
			}

			throw new NotImplementedException();
		}

		public Task<int> ExecuteDmlAsync<T1>(QueryMode queryMode, Expression expression, CancellationToken cancellationToken)
		{
			return Task.FromResult(ExecuteDml<T1>(queryMode, expression));
		}

		public IFutureEnumerable<TResult> ExecuteFuture<TResult>(Expression expression)
		{
			return new FutureEnumerable<TResult>(CreateQuery<TResult>(expression));
		}

		public IFutureValue<TResult> ExecuteFutureValue<TResult>(Expression expression)
		{
			throw new NotImplementedException();
		}

		public void SetResultTransformerAndAdditionalCriteria(IQuery query, NhLinqExpression nhExpression, IDictionary<string, Tuple<object, IType>> parameters)
		{
			throw new NotImplementedException();
		}

		private object ExecuteInMemoryQuery(Expression expression, bool isEnumerable)
		{
			var newExpr = new ExpressionTreeModifier<T>(Source).Visit(expression);

			if (isEnumerable)
			{
				return Source.Provider.CreateQuery(newExpr);
			}

			return Source.Provider.Execute(newExpr);
		}
	}
}
