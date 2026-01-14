namespace SOK.Domain.Interfaces
{
    /// <summary>
    /// Marker interface dla encji, które mogą mieć dodatkowe metadane w ParishInfo.
    /// </summary>
    public interface IEntityMetadata
    {
        int Id { get; }
    }
}