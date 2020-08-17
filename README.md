# SubstituteQueryable

Helper to ease unit testing when you are working with NHibernate's ISession, particularly querying with fetching

For example if you have a mocked (e.g. with NSubstitute) ISession named session then you can do something like 

by passing in an IQueryable<T>

    var people = GetTestPeople();
    
    session.Query<Person>().Returns(new SubstituteQueryable<Person>(people));
    
or a params array of items
    
    session.Query<Cat>().Returns(new SubstituteQueryable<Cat>(new Cat("Mog"), new Cat("Garfield"));
