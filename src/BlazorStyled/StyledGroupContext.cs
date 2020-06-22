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

        public void RegisterLoadTask(Task task)
        {
            if (task.IsCompleted)
            {
                return;
            }

            SetLoading(true);
            task.ContinueWith(OnLoadTaskCompleted);
            Interlocked.Increment(ref mTaskCounter);
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

            Loading = value;
            OnLoadingChanged?.Invoke(value);
        }
    }
}
