namespace SubstituteQueryable
{
	using System.Linq.Expressions;

	class SourceProcessor : ExpressionVisitor
	{
		readonly object _item;

		public SourceProcessor(object item)
		{
			_item = item;
		}
		protected override Expression VisitUnary(UnaryExpression node)
		{
			return Visit(node.Operand);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return Expression.Constant(_item);
		}
	}
}
