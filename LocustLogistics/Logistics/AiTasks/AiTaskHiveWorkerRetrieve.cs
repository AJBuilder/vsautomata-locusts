using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.Essentials;

namespace LocustLogistics.TransferItems.AiTasks
{
    public class AiTaskHiveWorkerRetrieve : IAiTask
    {
        float priority;
        EntityAgent entity;
        float slot;
        AnimationMetaData travellingAnimation;
        AnimationMetaData accessAnimation;
        WaypointsTraverser pathTraverser;


        public string Id => "logisticsorder";

        public int Slot => 0;

        public float Priority => priority;

        public float PriorityForCancel => priority;

        public string ProfilerName { get; set; }


        public AiTaskHiveWorkerRetrieve(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        {
            this.entity = entity;
            priority = taskConfig["priority"].AsFloat();
            slot = (int)taskConfig["slot"]?.AsInt(0);

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

        }


        public void AfterInitialize()
        {
        }

        public bool CanContinueExecute()
        {
            return ShouldExecute();
        }

        public bool ContinueExecute(float dt)
        {
            return true;
        }

        public void FinishExecute(bool cancelled)
        {
            pathTraverser.Stop();
        }

        public bool ShouldExecute()
        {
            // Return true if there is a retrieve order for this
            return true;
        }

        public void StartExecute()
        {
            //pathTraverser.NavigateTo_Async(retrieveStack.From.Position, 1f, 1.1f, () =>
            //{
            //}, null);
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
