namespace SubstituteQueryable
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using Remotion.Linq.Parsing.ExpressionVisitors;

	class BlockVisitor<T> : ExpressionVisitor
	{
		readonly T _target;

		public BlockVisitor(T target)
		{
			_target = target;
		}

		protected override Expression VisitBlock(BlockExpression node)
		{
			foreach (var item in node.Expressions.OfType<BinaryExpression>())
			{
				var property = Expression.Property(Expression.Constant(_target), (item.Left as ParameterExpression).Name);

				var result = Expression.Convert(item.Right, property.Type);

				var assignment = Expression.Assign(property, result);

				Visit(assignment);
			}

			return base.VisitBlock(node);
		}
	}
	class BlockVisitor<Source, Target> : ExpressionVisitor
	{
		readonly Source _source;
		readonly Target _target;

		public BlockVisitor(Source source, Target target)
		{
			_source = source;
			_target = target;
		}

		protected override Expression VisitBlock(BlockExpression node)
		{
			foreach (var item in node.Expressions.OfType<BinaryExpression>())
			{
				var targetExpression = Expression.Property(Expression.Constant(_target), (item.Left as ParameterExpression).Name);

				var sourceExpression = new UnaryVisitor<Source>(_source).Visit(item.Right);

				var assignment = Expression.Assign(targetExpression, sourceExpression);

				Expression.Lambda(assignment).Compile().DynamicInvoke();
			}

			return base.VisitBlock(node);
		}
	}

	class UnaryVisitor<T> : ExpressionVisitor
	{
		readonly T _item;

		public UnaryVisitor(T item)
		{
			_item = item;
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			if (node.Operand.NodeType == ExpressionType.MemberAccess)
			{
				return Expression.Property(Expression.Constant(_item), (node.Operand as MemberExpression).Member as PropertyInfo);
			}

			if (node.Operand.NodeType == ExpressionType.Parameter)
			{
				return Expression.Constant(_item);
			}

			if (node.Operand is BinaryExpression binaryExpression)
			{
				var left = new ParameterReplacer(Expression.Parameter(binaryExpression.Left.Type)).Visit(binaryExpression);
				var right = new ParameterReplacer(Expression.Parameter(binaryExpression.Right.Type)).Visit(binaryExpression);

				return Expression.Constant(binaryExpression.Method.Invoke(_item, new object[] { "Cora", right }));
			}

			throw new Exception(node.Operand.NodeType.ToString());
		}
	}
	internal class ParameterReplacer : ExpressionVisitor
	{
		private readonly ParameterExpression _parameter;

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return base.VisitParameter(_parameter);
		}

		internal ParameterReplacer(ParameterExpression parameter)
		{
			_parameter = parameter;
		}
	}

}
