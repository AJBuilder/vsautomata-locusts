using Vintagestory.API.MathTools;

namespace LocustLogistics.Core.Interfaces
{
    public interface IHiveMember
    {
        Vec3d Position { get; }
        int Dimension { get; }
        public LocustHive Hive { get; set; }

    }
}
