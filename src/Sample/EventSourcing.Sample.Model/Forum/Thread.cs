using System;
using System.Collections.Generic;
using CodeSharp.EventSourcing;

namespace EventSourcing.Sample.Model.Forum
{
    public class Thread : AggregateRoot<Guid>
    {
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public Guid ForumId { get; private set; }
        public Guid AuthorId { get; private set; }
        public int Marks { get; private set; }
        public ThreadStatus Status { get; private set; }
        public StickInfo StickInfo { get; private set; }
        public DateTime CreateTime { get; private set; }

        public Thread() { }
        public Thread(string subject, string body, Forum forum, User author, int marks) : base(Guid.NewGuid())
        {
            Assert.IsValidKey(subject);
            Assert.Greater(marks, 0);
            OnEvent(new ThreadCreated(
                Id,
                subject,
                body,
                forum.Id,
                author.Id,
                marks,
                ThreadStatus.Normal,
                new StickInfo(false, null),
                DateTime.Now));
        }

        public void ChangeContent(string subject, string body, int marks)
        {
            OnEvent(new ContentChanged(Id, subject, body, marks));
        }
        public void MarkAsRecommended()
        {
            OnEvent(new ThreadStatusChanged(Id, ThreadStatus.Recommended));
        }
        public void UnMarkAsRecommended()
        {
            OnEvent(new ThreadStatusChanged(Id, ThreadStatus.Normal));
        }
        public void Close()
        {
            OnEvent(new ThreadStatusChanged(Id, ThreadStatus.Closed));
        }
        public void MarkAsDeleted()
        {
            OnEvent(new ThreadStatusChanged(Id, ThreadStatus.Deleted));
        }
        public void Stick()
        {
            OnEvent(new ThreadStickInfoChanged(Id, new StickInfo(true, DateTime.Now)));
        }
        public void CancelStick()
        {
            OnEvent(new ThreadStickInfoChanged(Id, new StickInfo(false, null)));
        }
    }
    public enum ThreadStatus
    {
        Normal = 1,        //一般帖子
        Recommended = 2,   //推荐帖子
        Closed = 3,        //已关闭帖子
        Deleted = 4,       //已删除帖子，逻辑删除
    }
    public class StickInfo : ValueObject<StickInfo>
    {
        public StickInfo() { }
        public StickInfo(bool isStick, DateTime? stickDate)
        {
            if (isStick)
            {
                Assert.IsNotNull(stickDate);
            }
            else
            {
                Assert.IsNull(stickDate);
            }
            IsStick = isStick;
            StickDate = stickDate;
        }

        public bool IsStick { get; private set; }
        public DateTime? StickDate { get; private set; }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return IsStick;
            yield return StickDate;
        }
    }

    [Event]
    public class ThreadCreated
    {
        public Guid Id { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public Guid ForumId { get; private set; }
        public Guid AuthorId { get; private set; }
        public int Marks { get; private set; }
        public ThreadStatus Status { get; private set; }
        public StickInfo StickInfo { get; private set; }
        public DateTime CreateTime { get; private set; }

        public ThreadCreated(Guid id, string subject, string body, Guid forumId, Guid authorId, int marks, ThreadStatus status, StickInfo stickInfo, DateTime createTime)
        {
            Id = id;
            Subject = subject;
            Body = body;
            ForumId = forumId;
            AuthorId = authorId;
            Marks = marks;
            Status = status;
            StickInfo = stickInfo;
            CreateTime = createTime;
        }
    }
    [Event]
    public class ContentChanged
    {
        public Guid Id { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public int Marks { get; private set; }

        public ContentChanged(Guid id, string subject, string body, int marks)
        {
            Id = id;
            Subject = subject;
            Body = body;
            Marks = marks;
        }
    }
    [Event]
    public class ThreadStatusChanged
    {
        public Guid Id { get; private set; }
        public ThreadStatus Status { get; private set; }

        public ThreadStatusChanged(Guid id, ThreadStatus status)
        {
            Id = id;
            Status = status;
        }
    }
    [Event]
    public class ThreadStickInfoChanged
    {
        public Guid Id { get; private set; }
        public StickInfo StickInfo { get; private set; }

        public ThreadStickInfoChanged(Guid id, StickInfo stickInfo)
        {
            Id = id;
            StickInfo = stickInfo;
        }
    }
}
