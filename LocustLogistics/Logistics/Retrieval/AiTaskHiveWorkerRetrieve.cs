using LocustLogistics.Core;
using LocustLogistics.Logistics.Storage;
using LocustLogistics.Nests;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.Essentials;
using Vintagestory.GameContent;

namespace LocustLogistics.Logistics.Retrieval
{
    public class AiTaskHiveWorkerRetrieve : IAiTask
    {
        float priority;
        EntityAgent entity;
        AnimationMetaData travellingAnimation;
        AnimationMetaData accessAnimation;
        WaypointsTraverser pathTraverser;
        RetrievalLogisticsModSystem modSystem;
        IReadOnlyDictionary<IHiveLogisticsWorker, RetrievalRequest> assignments;
        LocustHivesModSystem hivesSystem;
        IHiveLogisticsWorker member;
        RetrievalRequest request;
        bool pathfindingActive;


        public string Id => "logisticsorder";

        public int Slot => 0;

        public float Priority => priority;

        public float PriorityForCancel => priority;

        public string ProfilerName { get; set; }


        public AiTaskHiveWorkerRetrieve(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        {
            this.entity = entity;
            priority = taskConfig["priority"].AsFloat();

            JsonObject travellingAnimationCode = taskConfig["travellingAnimation"];
            if (travellingAnimationCode.Exists)
            {
                var code = travellingAnimationCode.AsString()?.ToLowerInvariant();
                travellingAnimation = this.entity.Properties.Client.Animations.FirstOrDefault(a => a.Code == code);
            }

            JsonObject accessAnimationCode = taskConfig["accessAnimation"];
            if (accessAnimationCode.Exists)
            {
                var code = accessAnimationCode.AsString()?.ToLowerInvariant();
                accessAnimation = this.entity.Properties.Client.Animations.FirstOrDefault(a => a.Code == code);
            }

            // TODO: Sounds for travelling, access, and finishing

            pathTraverser = entity.GetBehavior<EntityBehaviorTaskAI>().PathTraverser;
            modSystem = entity.Api.ModLoader.GetModSystem<RetrievalLogisticsModSystem>();
            assignments = modSystem.Assignments;
        }

        public void AfterInitialize()
        {
            member = entity as IHiveLogisticsWorker;
            if (member == null)
            {
                member = entity
                        .SidedProperties
                        .Behaviors
                        .OfType<IHiveLogisticsWorker>()
                        .FirstOrDefault();
            }
        }

        public bool ShouldExecute()
        {
            if (member == null ||
                !assignments.TryGetValue(member, out request)) return false;

            return false;
        }

        public void StartExecute()
        {
            if (request == null) return;

            pathfindingActive = true;

            if (travellingAnimation != null)
            {
                entity.AnimManager?.StartAnimation(travellingAnimation);
            }

            pathTraverser.NavigateTo_Async(
                request.From.Position,
                0.02f,
                1.0f,
                OnGoalReached,
                OnStuck
            );
        }

        public bool CanContinueExecute()
        {
            return true;
        }

        public bool ContinueExecute(float dt)
        {
            // Finish if no longer pathfinding.
            return pathfindingActive;
        }

        public void FinishExecute(bool cancelled)
        {
            pathTraverser.Stop();
            pathfindingActive = false;

            if (travellingAnimation != null)
            {
                entity.AnimManager?.StopAnimation(travellingAnimation.Code);
            }

        }

        private void OnGoalReached()
        {
            pathfindingActive = false;
        }

        private void OnStuck()
        {
            pathfindingActive = false;
        }

        public bool Notify(string key, object data)
        {
            return false;
        }

        public void OnEntityDespawn(EntityDespawnData reason)
        {
        }

        public void OnEntityHurt(DamageSource source, float damage)
        {
        }

        public void OnEntityLoaded()
        {
        }

        public void OnEntitySpawn()
        {
        }

        public void OnStateChanged(EnumEntityState beforeState)
        {
        }
    }
}
