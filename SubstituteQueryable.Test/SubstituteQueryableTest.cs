namespace SubstituteQueryable.Test
{
	using System.Linq;
	using System.Threading.Tasks;
	using NHibernate.Linq;
	using NUnit.Framework;
	using SubstituteQueryable;

	[TestFixture]
	internal class SubstituteQueryableTest
	{
		SubstituteQueryable<Person> _queryable;

		[SetUp]
		public void SetUp()
		{
			var items = new[] {
				new Person { Name = "Cora", Parent = new Person { Name = "Dilip" } },
				new Person { Name = "Alice", Parent = new Person { Name = "Dilip" } },
				new Person { Name = "Bob", Children = new [] { new Person { Name = "Eva" }, new Person { Name = "Fatima" } } }
			};
			_queryable = new SubstituteQueryable<Person>(items);
		}

		[Test]
		public void VerifyOrderBy()
		{
			var result = _queryable.OrderBy(x => x.Name).ToList();

			Assert.That(result[0].Name, Is.EqualTo("Alice"));
			Assert.That(result[1].Name, Is.EqualTo("Bob"));
			Assert.That(result[2].Name, Is.EqualTo("Cora"));
		}

		[Test]
		public void VerifyOrderByDescending()
		{
			var result = _queryable.OrderByDescending(x => x.Name).ToList();

			Assert.That(result[0].Name, Is.EqualTo("Cora"));
			Assert.That(result[1].Name, Is.EqualTo("Bob"));
			Assert.That(result[2].Name, Is.EqualTo("Alice"));
		}

		[Test]
		public void VerifyFetch()
		{
			var result = _queryable.Fetch(x => x.Parent).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
		}

		[Test]
		public void VerifyFetchChain()
		{
			var result = _queryable.Fetch(x => x.Parent).ThenFetch(x => x.Parent).FetchMany(x => x.Children).ThenFetchMany(x => x.Children).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
		}

		[Test]
		public async Task VerifyToListAsync()
		{
			var result = await _queryable.ToListAsync();

			Assert.That(result, Has.Count.EqualTo(3));
		}

		[TestCase("Bob", ExpectedResult = true)]
		[TestCase("Veronica", ExpectedResult = false)]
		public bool VerifyAny(string name)
		{
			return _queryable.Any(x => x.Name == name);
		}

		[TestCase("Bob", ExpectedResult = true)]
		[TestCase("Veronica", ExpectedResult = false)]
		public Task<bool> VerifyAnyAsync(string name)
		{
			return _queryable.AnyAsync(x => x.Name == name);
		}

		[Test]
		public void VerifyToFuture()
		{
			var result = _queryable.ToFuture();

			Assert.That(result.Count(), Is.EqualTo(3));
			Assert.That(result.ElementAt(0).Name, Is.EqualTo("Cora"));
			Assert.That(result.ElementAt(1).Name, Is.EqualTo("Alice"));
			Assert.That(result.ElementAt(2).Name, Is.EqualTo("Bob"));
		}

		[Test]
		public void VerifyToFutureValue()
		{
			var result = _queryable.ToFutureValue();

			Assert.That(result.Value.Name, Is.EqualTo("Cora"));
		}

		[Test]
		public void VerifyDelete()
		{
			var result = _queryable.Where(x => x.Name == "Cora").Delete();

			Assert.That(result, Is.EqualTo(1));
			Assert.That(_queryable.Deletes, Has.Count.EqualTo(1));
			Assert.That(_queryable.Deletes[0].Name, Is.EqualTo("Cora"));
		}

		[Test]
		public async Task VerifyDeleteAsync()
		{
			var result = await _queryable.Where(x => x.Name == "Cora").DeleteAsync(default);

			Assert.That(result, Is.EqualTo(1));
			Assert.That(_queryable.Deletes, Has.Count.EqualTo(1));
			Assert.That(_queryable.Deletes[0].Name, Is.EqualTo("Cora"));
		}

		[Test]
		public void VerifyInsertInto()
		{
			var result = _queryable.Where(x => x.Name == "Cora").InsertInto(x => new Employee { Name = x.Name, Person = x });

			Assert.That(result, Is.EqualTo(1));
			Assert.That(_queryable.Inserts, Has.Count.EqualTo(1));
			Assert.That(_queryable.Inserts[0], Has.Property("Name").EqualTo("Cora"));
			Assert.That(_queryable.Inserts[0], Has.Property("Person").Not.Null.With.Property("Name").EqualTo("Cora"));
		}

		[Test]
		public async Task VerifyInsertIntoAsync()
		{
			var result = await _queryable.Where(x => x.Name == "Cora").InsertIntoAsync(x => new Employee { Name = x.Name, Person = x }, default);

			Assert.That(result, Is.EqualTo(1));
			Assert.That(_queryable.Inserts, Has.Count.EqualTo(1));
			Assert.That(_queryable.Inserts[0], Has.Property("Name").EqualTo("Cora"));
			Assert.That(_queryable.Inserts[0], Has.Property("Person").Not.Null.With.Property("Name").EqualTo("Cora"));
		}

		[Test]
		public void VerifyUpdate()
		{
			var result = _queryable.Where(x => x.Name == "Cora").Update(x => new Person { Name = x.Name + "2" });

			Assert.That(result, Is.EqualTo(1));
			Assert.That(_queryable.Updates, Has.Count.EqualTo(1));
			Assert.That(_queryable.Updates[0], Has.Property("Name").EqualTo("Cora2"));
		}

		[Test]
		public async Task VerifyUpdateAsync()
		{
			var result = await _queryable.Where(x => x.Name == "Cora").UpdateAsync(x => new Person { Name = x.Name + "2" }, default);

			Assert.That(result, Is.EqualTo(1));
			Assert.That(_queryable.Updates, Has.Count.EqualTo(1));
			Assert.That(_queryable.Updates[0], Has.Property("Name").EqualTo("Cora2"));
		}
	}
}
