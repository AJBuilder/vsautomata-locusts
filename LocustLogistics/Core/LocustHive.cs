using LocustLogistics.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace LocustLogistics.Core
{

    public class LocustHive
    {
        private readonly HashSet<IHiveMember> _members = new();
        private readonly HashSet<ILocustNest> _nests = new();

        // Events
        public event Action<IHiveMember> MemberTuned;
        public event Action<IHiveMember> MemberDetuned;

        public long Id { get; }
        public IEnumerable<IHiveMember> Members => _members;
        public IEnumerable<ILocustNest> Nests => _nests;


        public LocustHive(long id)
        {
            Id = id;
        }

        //public IHiveStorage NearestStorage(Vec3d position, IHiveMember except)
        //{
        //    IHiveStorage targetStorage = null;
        //    float closestDistance = float.MaxValue;
        //
        //    foreach (var storage in _storages)
        //    {
        //        if (storage != from && storage.AvailableHiveStorage > 0)
        //        {
        //            var dist = from.Position.SquareDistanceTo(storage.Position);
        //            if (dist < closestDistance)
        //            {
        //                targetStorage = storage;
        //                closestDistance = dist;
        //            }
        //        }
        //    }
        //
        //}
        //
        ///// <summary>
        ///// Queue an assignment for this stack to be pushed to storage somewhere in the hive.
        ///// </summary>
        ///// <param name="stact"></param>
        ///// <param name="from"></param>
        ///// <returns></returns>
        //public static RetrieveOrder PushToStorage(ItemStack stack, IHiveStorage from)
        //{
        //    // Find the closest storage location that has available space (excluding the source storage)
        //    
        //
        //    // If no suitable storage found, return false
        //    if (targetStorage == null)
        //    {
        //        return false;
        //    }
        //
        //    // Order a worker to transfer the stack to the target storage
        //    var worker = TransferStack(stack, from, targetStorage);
        //
        //    // Return true if a worker was successfully assigned
        //    return worker != null;
        //}
        //
        ///// <summary>
        ///// Order a worker to transfer a stack.
        ///// Returns the worker ordered. If not able to find one, returns null.
        ///// </summary>
        ///// <param name="stack"></param>
        ///// <param name="from"></param>
        ///// <param name="to"></param>
        ///// <returns></returns>
        //public IHiveWorker TransferStack(ItemStack stack, IHiveStorage from, IHiveStorage to)
        //{
        //    var order = new RetrieveOrder(stack, from, to);
        //
        //    // Find the closest worker that doesn't have an order
        //    IHiveWorker closestWorker = null;
        //    float closestWorkerDist = float.MaxValue;
        //    foreach (var worker in _workers)
        //    {
        //        if (worker.Assignment == null)
        //        {
        //            var dist = from.Position.SquareDistanceTo(worker.Position);
        //            if (dist < closestWorkerDist)
        //            {
        //                closestWorker = worker;
        //                closestWorkerDist = dist;
        //            }
        //        }
        //    }
        //
        //    // Find the closest nest that contains a worker
        //    ILocustNest closestNest = null;
        //    float closestNestDist = float.MaxValue;
        //    foreach (var nest in _nests)
        //    {
        //        var locusts = nest.StoredLocusts;
        //        if (locusts != null && locusts.Count > 0)
        //        {
        //            var dist = from.Position.SquareDistanceTo(nest.Position);
        //            if(dist < closestNestDist)
        //            {
        //                closestNest = nest;
        //                closestNestDist = dist;
        //            }
        //        }
        //    }
        //
        //
        //    IHiveWorker assignedWorker = null;
        //    if(closestNestDist > closestWorkerDist && closestNest != null)
        //    {
        //        assignedWorker = closestNest.StoredLocusts.First();
        //        closestNest.StoredLocusts.Remove(assignedWorker);
        //        assignedWorker.OnUnstore();
        //    } else
        //    {
        //        assignedWorker = closestWorker;
        //    }
        //
        //    if(assignedWorker != null)
        //    {
        //        assignedWorker.Assignment = order;
        //    }
        //
        //
        //    return assignedWorker;
        //}


        public void Tune(IHiveMember member)
        {
            // Remove from existing
            LocustHive existing = member.Hive;
            existing?.Detune(member);

            _members.Add(member);

            // Presort
            if(member is ILocustNest nest)
            {
                _nests.Add(nest);
            }
            member.Hive = this;

            MemberTuned?.Invoke(member);
        }

        public void Detune(IHiveMember member)
        {
            _members.Remove(member);
            if (member is ILocustNest nest)
            {
                _nests.Remove(nest);
            }
            member.Hive = null;

            MemberDetuned?.Invoke(member);
        }
    }

}
