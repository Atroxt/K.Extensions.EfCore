using K.Extensions.EfCore.Models;

namespace K.Extensions.EfCore.Tests.Entities
{
    public class EntityWithSoftDelete : IAmSoftdeleteAble
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; private set; }
    }
    public class EntityWithSoftDeleteAndDeletedAt : IAmSoftdeleteAble, IHaveDeletionTime
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }
    }
    public class EntityWithCreationTime : IAmSoftdeleteAble, IHaveCreationTime
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
    }
    public class EntityWithModificationTime : IHaveModificationTime
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime? ModifiedAtUtc { get; private set; }
    }
    public class EntityWithAuditing : IAmAuditAble, IAmSoftdeleteAble
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; private set; }
        public string Name { get; set; }
    }
}
