using System.Collections.Generic;
using Nobisoft.Core.Domain.ValueObjects;

namespace Nobisoft.Core.Domain.Interfaces
{
    public interface IHasExtraProperties
    {
        ICollection<ExtraProperty> ExtraProperties { get; set; }
    }
}