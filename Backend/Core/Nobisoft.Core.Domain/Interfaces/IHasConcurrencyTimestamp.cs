namespace Nobisoft.Core.Domain.Interfaces
{
    /// <summary>
    /// Define concurrency field
    /// </summary>
    public interface IHasConcurrencyTimestamp
    {
        public uint ConcurrencyStamp { get; set; }
    }
}