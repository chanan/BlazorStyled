using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace BlazorStyled
{
    public class StyledGroupContext
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<Task> mLoadTasks = new List<Task>();
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
            mLoadTasks.Add(task);
        }

        private void OnLoadTaskCompleted(Task completedTask)
        {
            mLoadTasks.Remove(completedTask);
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
