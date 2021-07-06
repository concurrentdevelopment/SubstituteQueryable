namespace SubstituteQueryable
{
	using System.Linq.Expressions;
	using System.Reflection;

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

		public override Expression Visit(Expression node)
		{
			if (node.NodeType == ExpressionType.MemberAccess)
			{
				return Expression.Property(Expression.Constant(_item), (node as MemberExpression).Member as PropertyInfo);
			}

			if (node.NodeType == ExpressionType.Parameter)
			{
				return Expression.Constant(_item);
			}

			if (node.NodeType == ExpressionType.Constant)
			{
				return node;
			}

			if (node is BinaryExpression binaryExpression)
			{
				var left = Expression.Lambda(Visit(binaryExpression.Left)).Compile().DynamicInvoke();
				var right = Expression.Lambda(Visit(binaryExpression.Right)).Compile().DynamicInvoke();

				return Expression.Constant(binaryExpression.Method.Invoke(_item, new object[] { left, right }));
			}

			return base.Visit(node);
		}
	}
}
