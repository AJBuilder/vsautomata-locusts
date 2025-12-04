using LocustLogistics.Nests;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

#nullable disable warnings

namespace LocustLogistics.Core
{
    /// <summary>
    /// This core mod system has to do with the tuning and synchronizing of "members" within a "hive".
    /// </summary>
    public class LocustHivesModSystem : ModSystem
    {
        // Events
        public event Action<IHiveMember, int?, int?> MemberTuned;


        int nextId;
        Dictionary<IHiveMember, int> allMembers = new Dictionary<IHiveMember, int>();
        Dictionary<int, HashSet<IHiveMember>> hiveMembers = new Dictionary<int, HashSet<IHiveMember>>();

        public IReadOnlyDictionary<IHiveMember, int> Membership => allMembers;

        public IReadOnlySet<IHiveMember> GetHiveMembers(int hive)
        {
            if (hiveMembers.TryGetValue(hive, out var nests)) return nests;
            else return new HashSet<IHiveMember>();
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemHiveTuner", typeof(ItemHiveTuner));
            api.RegisterEntityBehaviorClass("hivetunable", typeof(EntityBehaviorHiveTunable));
            api.RegisterBlockEntityBehaviorClass("HiveTunable", typeof(BEBehaviorHiveTunable));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            api.Event.GameWorldSave += () => api.WorldManager.SaveGame.StoreData("nextHiveId", nextId);
            api.Event.SaveGameLoaded += () => nextId = api.WorldManager.SaveGame.GetData("nextHiveId", 0);
        }

        /// <summary>
        /// Add a member to a hive.
        /// Will trigger the MemberTuned event and call OnTuned for the member.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="member"></param>
        public void Tune(int? hive, IHiveMember member)
        {
            // If already tuned
            if (allMembers.TryGetValue(member, out int prevHive))
            {
                // Bail if same hive
                if (hive.HasValue && hive.Value == prevHive) return;

                // Delete all relationships
                allMembers.Remove(member);

                // Cleanup member caching
                if (hiveMembers.TryGetValue(prevHive, out var members))
                {
                    members.Remove(member);
                    if (members.Count == 0) hiveMembers.Remove(prevHive);
                }

            }

            if (hive.HasValue)
            {
                var val = hive.Value;
                allMembers[member] = val;

                // Cache reverse relationship
                if (!this.hiveMembers.TryGetValue(val, out var members))
                {
                    members = new HashSet<IHiveMember>();
                    this.hiveMembers[val] = members;
                }
                members.Add(member);

            }

            member.WasTuned(prevHive, hive);
            MemberTuned?.Invoke(member, prevHive, hive);
        }


        /// <summary>
        /// Creates a new hive that doesn't exist yet.
        /// Note: Not a perfect allocator as there is no explicit check.
        ///       Not guaranteed to work after int.MaxValue.
        /// </summary>
        /// <param name="firstMember"></param>
        /// <returns></returns>
        public int CreateHive()
        {
            // Get the next (hopefully) free id.
            while (hiveMembers.ContainsKey(nextId))
                nextId++;

            // Post increment for the next time.
            return nextId++;
        }

    }
}
