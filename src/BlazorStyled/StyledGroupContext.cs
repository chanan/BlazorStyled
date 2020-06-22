using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace BlazorStyled
{
    public class StyledGroupContext
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<Task, object> mLoadTasks = new ConcurrentDictionary<Task, object>();
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
            mLoadTasks.TryAdd(task, null);
        }

        private void OnLoadTaskCompleted(Task completedTask)
        {
            mLoadTasks.TryRemove(completedTask, out _);
            if (mLoadTasks.Count == 0)
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
