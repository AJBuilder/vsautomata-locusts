using LocustLogistics.Core;
using LocustLogistics.Logistics.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;

#nullable disable warnings

namespace LocustLogistics.Logistics.Retrieval
{
    public class RetrievalLogisticsModSystem : ModSystem
    {

        Dictionary<IHiveLogisticsWorker, RetrievalRequest> assignments = new Dictionary<IHiveLogisticsWorker, RetrievalRequest>();
        Dictionary<RetrievalRequest, IHiveLogisticsWorker> activeRequests = new Dictionary<RetrievalRequest, IHiveLogisticsWorker>();
        Queue<RetrievalRequest> queuedRequests = new Queue<RetrievalRequest>();


        LogisticsWorkersModSystem workerSystem;
        StorageModSystem storageSystem;

        public IReadOnlyDictionary<IHiveLogisticsWorker, RetrievalRequest> Assignments => assignments;

        public IReadOnlyDictionary<RetrievalRequest, IHiveLogisticsWorker> ActiveRequests => activeRequests;

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            AiTaskRegistry.Register<AiTaskHiveWorkerRetrieve>("retrieveHiveStack");
            workerSystem = api.ModLoader.GetModSystem<LogisticsWorkersModSystem>();

            api.Event.RegisterGameTickListener((dt) =>
            {
                // Iterate over current queue (or max of 10) and try to assign.
                // TODO: Better optimization than capping number of processed requests to 10.
                int count = Math.Min(queuedRequests.Count, 10);
                for (int i = 0; i < count; i++)
                {
                    AssignOrQueueRequest(queuedRequests.Dequeue());
                }
            }, 3000);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public RetrievalRequest Request(ItemStack stack, IHiveStorage from, Action onCompletion = null, Action onFailure = null)
        {
            RetrievalRequest request = null;
            request = new RetrievalRequest(stack, from);
            request.CompletedEvent += () =>
                {
                    if (activeRequests.TryGetValue(request, out var worker)) assignments.Remove(worker);
                    onCompletion?.Invoke();
                };
            request.AbandonedEvent += () =>
                {
                    if (activeRequests.TryGetValue(request, out var worker)) assignments.Remove(worker);
                    queuedRequests.Enqueue(request);
                };
            request.FailedEvent += () =>
                {
                    if (activeRequests.TryGetValue(request, out var worker)) assignments.Remove(worker);
                    onFailure?.Invoke();
                };
            request.CancelledEvent += () =>
                {
                    if (activeRequests.TryGetValue(request, out var worker)) assignments.Remove(worker);

                    // Cancelling shouldn't happen often?
                    // Let's just naively iterate and remove it.
                    int count = queuedRequests.Count;
                    for (int i = 0; i < count; i++)
                    {
                        RetrievalRequest current = queuedRequests.Dequeue();
                        if (current != request)
                        {
                            queuedRequests.Enqueue(current);
                        }
                    }
                };

            AssignOrQueueRequest(request);
            return request;
        }


        private void AssignOrQueueRequest(RetrievalRequest request)
        {
            // Queue if not part of a hive
            if (!storageSystem.Membership.TryGetValue(request.From, out var hive))
            {
                queuedRequests.Enqueue(request);
            }

            // Enumerate over workers that don't have assignments and are closest.
            var hiveWorkers = workerSystem.GetHiveWorkers(hive)
                // Filter out workers already assigned
                .Where(w => !assignments.ContainsKey(w))
                // Order them by distance to the request source
                .OrderBy(w => w.Position.DistanceTo(request.From.Position));

            foreach (var worker in hiveWorkers)
            {
                // Attempt assignment
                if (worker.TryAssignRetrievalRequest(request))
                {
                    assignments[worker] = request;
                    activeRequests[request] = worker;
                    return;
                }
                // else: worker refused, try next best
            }

            // Nobody accepted, queue for later
            queuedRequests.Enqueue(request);
        }

    }
}
