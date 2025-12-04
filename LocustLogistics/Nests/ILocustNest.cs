using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace LocustLogistics.Nests
{
    public interface ILocustNest
    {
        Vec3d Position { get; }
        int Dimension { get; }
        public int MaxCapacity { get; }
        public bool HasRoom { get; }
        public bool TryStoreLocust(EntityLocust locust);
        public bool TryUnstoreLocust(int index);
    }
}
