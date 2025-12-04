using LocustLogistics.Logistics.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace LocustLogistics.Logistics.Retrieval
{
    public class RetrievalRequest
    {
        bool completed;
        bool failed;

        public event Action CompletedEvent;
        public event Action AbandonedEvent;
        public event Action FailedEvent;
        public event Action CancelledEvent;
        public ItemStack Stack { get; }
        public IHiveStorage From { get; }

        public bool Completed => completed;

        public bool Failed => failed;

        public RetrievalRequest(ItemStack stack, IHiveStorage from)
        {
            Stack = stack;
            From = from;
        }

        /// <summary>
        /// Called to indicate that request was completed.
        /// </summary>
        public void Complete()
        {
            if (!Failed && !Completed)
            {
                CompletedEvent?.Invoke();
            }
        }
        public void Abandon()
        {
            if (!Failed && !Completed)
            {
                AbandonedEvent?.Invoke();
            }
        }
        public void Fail()
        {
            if (!Failed && !Completed)
            {
                FailedEvent?.Invoke();
            }
        }

        public void Cancel()
        {
            if (!Failed && !Completed)
            {
                CancelledEvent?.Invoke();
            }
        }

    }
}
