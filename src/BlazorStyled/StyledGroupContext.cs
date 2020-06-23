using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace BlazorStyled
{
    public class StyledGroupContext
    {
        private int mTaskCounter;
        public bool Loading { get; set; } = true;
        public event Action<bool> OnLoadingChanged;
        private bool mRendering = true;

        public void SetRendering(bool value)
        {
            if (value == mRendering)
            {
                return;
            }

            mRendering = value;

            // Rendering is finished, there are no running tasks and Loading is still true
            // This can happen when all registered tasks are completed synchronously and none of them are actually registered
            if (!mRendering && mTaskCounter == 0)
            {
                SetLoading(false);
            }
        }

        public void RegisterLoadTask(Task task)
        {
            if (task.IsCompleted)
            {
                return;
            }

            SetLoading(true);
            Interlocked.Increment(ref mTaskCounter);
            task.ContinueWith(OnLoadTaskCompleted);
        }

        private void OnLoadTaskCompleted(Task _)
        {
            Interlocked.Decrement(ref mTaskCounter);
            if (mTaskCounter == 0)
            {
                SetLoading(false);
            }
        }

        private void SetLoading(bool value)
        {
            if (Loading == value)
            {
                return;
            }

            // Trying to finish loading before OnAfterRender call - handled in SetRendering
            if (!value && mRendering)
            {
                return;
            }

            Loading = value;
            OnLoadingChanged?.Invoke(value);
        }
    }
}
