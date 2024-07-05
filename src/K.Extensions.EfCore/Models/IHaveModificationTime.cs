namespace K.Extensions.EfCore.Models;

public partial interface IHaveModificationTime
{
    DateTime? ModifiedAtUtc { get; }
}