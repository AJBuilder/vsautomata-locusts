using System.Collections.Generic;
using Vintagestory.GameContent;

namespace LocustLogistics.Core.Interfaces
{
    public interface ILocustNest : IHiveMember
    {
        ISet<EntityLocust> StoredLocusts { get; }
    }
}
