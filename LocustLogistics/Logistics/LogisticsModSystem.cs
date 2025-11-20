using LocustLogistics.Core;
using LocustLogistics.Core.Interfaces;
using LocustLogistics.Logistics.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;

namespace LocustLogistics.TransferItems
{
    public class LogisticsModSystem : ModSystem
    {
        private readonly ConditionalWeakTable<LocustHive, HashSet<IHiveStorage>> _hiveStorages = new();
        private AutomataLocustsCore _core;

        public override void Start(ICoreAPI api)
        {
            _core = api.ModLoader.GetModSystem<AutomataLocustsCore>();
            _core.MemberTuned += OnMemberTuned;
            _core.MemberDetuned += OnMemberDetuned;
        }

        private void OnMemberTuned(LocustHive hive, IHiveMember member)
        {
            if (member is IHiveStorage storage)
            {
                var storages = _hiveStorages.GetValue(hive, _ => new HashSet<IHiveStorage>());
                storages.Add(storage);
            }
        }

        private void OnMemberDetuned(LocustHive hive, IHiveMember member)
        {
            if (member is IHiveStorage storage)
            {
                if (_hiveStorages.TryGetValue(hive, out var storages))
                {
                    storages.Remove(storage);
                }
            }
        }

        public IEnumerable<IHiveStorage> GetStorages(LocustHive hive)
        {
            if (_hiveStorages.TryGetValue(hive, out var storages))
            {
                return storages;
            }
            return Enumerable.Empty<IHiveStorage>();
        }
    }
}
