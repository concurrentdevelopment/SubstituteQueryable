namespace SubstituteQueryable
{
	using System;
	using System.Collections.Generic;
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
			var unary = translationExpression as UnaryExpression;

			var lambda = unary.Operand as LambdaExpression;

			var updates = lambda.Compile().DynamicInvoke(_source) as Dictionary<string, object>;

			foreach (var property in typeof(Target).GetProperties())
			{
				if (updates.TryGetValue(property.Name, out var propertyValue))
				{
					property.SetValue(_target, propertyValue);
				}
			}

			return _target;
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			return Visit(node.Operand);
		}
	}
}
