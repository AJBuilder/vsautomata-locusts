using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace LocustLogistics.Util
{
    /// <summary>
    /// Very hacky custom BlockEntity class that calls an AfterInitialized method after initialization. Like the Entity class.
    /// </summary>
    public class AfterInitBlockEntity : BlockEntity
    {
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            foreach (var val in Behaviors) (val as IAfterInitialize)?.AfterInitialize();
        }
    }
}
