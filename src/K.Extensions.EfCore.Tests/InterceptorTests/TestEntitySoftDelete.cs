using K.Extensions.EfCore.Models;

namespace K.Extensions.EfCore.Tests.InterceptorTests
{
    public class TestEntitySoftDelete : IAmSoftdeleteAble
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; private set; }
    }
    public class TestEntitySoftDeleteAndDeletionTime : IAmSoftdeleteAble, IHaveDeletionTime
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }
    }
    public class TestEntityCreationTime : IHaveCreationTime
    {
        public Guid Id { get; set; }
        public DateTime CreatedAtUtc { get; private set; }
    }
    public class TestEntityModificationTime : IHaveModificationTime
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? ModifiedAtUtc { get; private set; }
    }
}
