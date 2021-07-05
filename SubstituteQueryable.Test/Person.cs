namespace SubstituteQueryable.Test
{
	using System.Collections.Generic;

	class Person
	{
		public string Name { get; set; }

		public Person Parent { get; set; }

		public IEnumerable<Person> Children { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}
