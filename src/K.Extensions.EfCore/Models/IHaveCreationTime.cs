namespace K.Extensions.EfCore.Models
{
    public partial interface IHaveCreationTime
    {
        DateTime CreatedAtUtc { get; }
    }
}
