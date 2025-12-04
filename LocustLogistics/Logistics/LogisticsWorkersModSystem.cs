using LocustLogistics.Logistics.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace LocustLogistics.Logistics
{
    public class LogisticsWorkersModSystem : ModSystem
    {

        Dictionary<IHiveLogisticsWorker, int> allTunedWorkers = new Dictionary<IHiveLogisticsWorker, int>();
        Dictionary<int, HashSet<IHiveLogisticsWorker>> hiveWorkers = new Dictionary<int, HashSet<IHiveLogisticsWorker>>();

        public IReadOnlyDictionary<IHiveLogisticsWorker, int> Membership => allTunedWorkers;

        public IReadOnlySet<IHiveLogisticsWorker> GetHiveWorkers(int hive)
        {
            if (hiveWorkers.TryGetValue(hive, out var workers)) return workers;
            else return new HashSet<IHiveLogisticsWorker>();
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterEntityBehaviorClass("hivelogisticsworker", typeof(EntityBehaviorLogisticsWorker));
        }

        public void UpdateLogisticsWorkerHiveMembership(IHiveLogisticsWorker worker, int? prevHive, int? hive)
        {
            if (hive.HasValue) allTunedWorkers[worker] = hive.Value;
            else allTunedWorkers.Remove(worker);

            // Clean up prior caching
            if (prevHive.HasValue)
            {
                if (hiveWorkers.TryGetValue(prevHive.Value, out var workers))
                {
                    workers.Remove(worker);
                    if (workers.Count == 0) hiveWorkers.Remove(prevHive.Value);
                }
            };

            // Add new caching
            if (hive.HasValue)
            {
                if (!hiveWorkers.TryGetValue(hive.Value, out var workers))
                {
                    workers = new HashSet<IHiveLogisticsWorker>();
                    hiveWorkers[hive.Value] = workers;
                }
                workers.Add(worker);
            }
        }

    }
}
