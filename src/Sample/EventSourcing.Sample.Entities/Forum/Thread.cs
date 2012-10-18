using System;

namespace EventSourcing.Sample.Entities
{
    public class ThreadEntity
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public Guid ForumId { get; set; }
        public Guid AuthorId { get; set; }
        public int Marks { get; set; }
        public ThreadStatus Status { get; set; }
        public bool IsStick { get; set; }
        public DateTime? StickDate { get; set; }
        public DateTime CreateTime { get; set; }
    }
    public enum ThreadStatus
    {
        Normal = 1,        //一般帖子
        Recommended = 2,   //推荐帖子
        Closed = 3,        //已关闭帖子
        Deleted = 4,       //已删除帖子，逻辑删除
    }
}
