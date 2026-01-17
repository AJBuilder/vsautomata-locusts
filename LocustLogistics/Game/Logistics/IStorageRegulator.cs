using Vintagestory.API.Common;

namespace LocustHives.Game.Logistics
{
    public interface IStorageRegulator
    {
        ItemStack TrackedItem { get; set; }
    }
}
