namespace Nobisoft.Core.Domain.ValueObjects
{
    /// <summary>
    /// To serve for extra properties if needed. It will be stored as a json value in the db, should use Postgresql
    /// </summary>
    public class ExtraProperty
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public object Value { get; set; }
    }
}