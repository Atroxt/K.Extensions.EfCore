namespace K.Extensions.EfCore.Models;

public partial interface IHaveDeletionTime
{
    DateTime? DeletedAtUtc { get; }
}