namespace SubstituteQueryable
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;

	internal class ObjectTranslator<Target> : ExpressionVisitor
	{
		private readonly object _source;
		private readonly Target _target;

		public ObjectTranslator(object source)
		{
			_source = source;
			_target = Activator.CreateInstance<Target>();
		}

		public Target GetObjectForDml(Expression translationExpression)
		{
			Visit(translationExpression);

			return _target;
		}

		protected override Expression VisitBlock(BlockExpression node)
		{
			foreach (var item in node.Expressions.OfType<BinaryExpression>())
			{
				var targetExpression = Expression.Property(Expression.Constant(_target), (item.Left as ParameterExpression).Name);

				var sourceExpression = new SourceProcessor(_source).Visit(item.Right);

				var assignment = Expression.Assign(targetExpression, sourceExpression);

				Expression.Lambda(assignment).Compile().DynamicInvoke();
			}

			return base.VisitBlock(node);
		}
	}
}
