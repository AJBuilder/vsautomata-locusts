using System;
using Vintagestory.API.MathTools;

#nullable enable

namespace LocustLogistics.Core
{
    public interface IHiveMember
    {

        /// <summary>
        /// Called to notify the member that they were tuned to a hive.
        /// </summary>
        /// <param name="prevHive"></param>
        /// <param name="hive"></param>
        void WasTuned(int? prevHive, int? hive);

    }
}
