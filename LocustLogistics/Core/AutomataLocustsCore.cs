using LocustLogistics.Core.Interfaces;
using LocustLogistics.Core.Items;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace LocustLogistics.Core
{
    /// <summary>
    /// This core mod system has to do with the tuning and synchronizing of "members" within a "hive".
    /// </summary>
    public class AutomataLocustsCore : ModSystem
    {
        // Events
        public event Action<LocustHive, IHiveMember> MemberTuned;
        public event Action<LocustHive, IHiveMember> MemberDetuned;

        long nextId;
        Dictionary<long, LocustHive> hives = new Dictionary<long, LocustHive>();

        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemHiveTuner", typeof(ItemHiveTuner));
        }

        public LocustHive GetHive(long id)
        {
            // Try to find it first
            if (hives.TryGetValue(id, out LocustHive hive))
            {
                return hive;
            }

            // Otherwise make one with the given id.
            return NewHiveWithId(id);
        }

        public LocustHive CreateHive()
        {
            return NewHiveWithId(nextId++);
        }

        private LocustHive NewHiveWithId(long id)
        {
            var newHive = new LocustHive(nextId++);
            hives[newHive.Id] = newHive;
            newHive.MemberTuned += (member) => MemberTuned?.Invoke(newHive, member);
            newHive.MemberDetuned += (member) => MemberDetuned?.Invoke(newHive, member);
            return newHive;
        }

    }
}
