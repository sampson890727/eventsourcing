using System;
using CodeSharp.EventSourcing;

namespace EventSourcing.Sample.Model.Forum
{
    public class User : AggregateRoot<Guid>
    {
        public string Name { get; private set; }

        public User() { }
        public User(string name) : base(Guid.NewGuid())
        {
            OnEvent(new UserCreated(Id, name));
        }
    }

    [Event]
    public class UserCreated
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public UserCreated(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
