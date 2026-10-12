using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.Domain.Commands
{
    public class CommandBase
    {
        public CommandBase(long tenantId, Tracker tracker)
        {
            TenantId = tenantId;
            Tracker = tracker;
        }

        public CommandBase()
        {
            
        }
        public long TenantId { get; set; }
        public Tracker Tracker { get; set; }
    }
}