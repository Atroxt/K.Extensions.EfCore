namespace K.Extensions.EfCore.Models
{
    public partial interface IAmSoftdeleteAble
    {
        bool IsDeleted { get; }
    }
}
