namespace SubstituteQueryable.Test
{
	using System;

	internal class Employee
	{
		public string Name { get; set; }

		public Person Person { get; set; }

		public Position Position { get; set; }

		public DateTime StartDate { get; set; }

		public string NextOfKinName { get; set; }
	}
}
