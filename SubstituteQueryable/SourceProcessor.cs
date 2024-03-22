namespace SubstituteQueryable
{
	using System.Linq.Expressions;

	internal class SourceProcessor : ExpressionVisitor
	{
		private readonly object _item;

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
