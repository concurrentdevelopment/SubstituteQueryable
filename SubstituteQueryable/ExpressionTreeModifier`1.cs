﻿namespace SubstituteQueryable
{
	using System.Linq;
	using System.Linq.Expressions;
	using NHibernate.Linq;

	internal class ExpressionTreeModifier<T> : ExpressionVisitor
	{
		private readonly IQueryable<T> _queryableData;

		internal ExpressionTreeModifier(IQueryable<T> queryableData)
		{
			_queryableData = queryableData;
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
			// Here the magic happens: the expression types are all NHibernateQueryableProxy,
			// so we replace them by the correct ones
			if (c.Type == typeof(SubstituteQueryable<T>))
				return Expression.Constant(_queryableData);
			else
				return c;
		}

		// https://stackoverflow.com/questions/39338308/implementing-a-custom-queryprovider-with-in-memory-query/42710376#42710376
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			//Don't overwrite if fetch wasn't the method being called
			if (!node.Method.Name.Equals(nameof(EagerFetchingExtensionMethods.Fetch))
				&& !node.Method.Name.Equals(nameof(EagerFetchingExtensionMethods.FetchMany))
				&& !node.Method.Name.Equals(nameof(EagerFetchingExtensionMethods.ThenFetch))
				&& !node.Method.Name.Equals(nameof(EagerFetchingExtensionMethods.ThenFetchMany)))
			{
				return base.VisitMethodCall(node);
			}

			//Get the first argument to the Fetch call. This would be our IQueryable or an Expression which returns the IQueryable.
			var fetchInput = node.Arguments[0];

			Expression returnExpression;

			switch (fetchInput.NodeType)
			{
				case ExpressionType.Constant:
					//If the input was a constant we need to run it through VisitConstant to get the underlying queryable from NHibernateQueryableProxy if applicable
					returnExpression = VisitConstant((ConstantExpression)fetchInput);
					break;
				default:
					//For everything else just return the input to fetch
					//This is covers cases if we do something like .Where(x).Fetch(x), here fetchInput would be another method call
					returnExpression = fetchInput;
					break;
			}

			return returnExpression;
		}
	}
}
