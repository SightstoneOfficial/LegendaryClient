using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

// astralfoxy:complete/threading/taskcallbackmanager.cs
namespace Complete.Threading
{
    class TaskCallbackManager
    {
        readonly ConcurrentDictionary<int, TaskCompletionSource<object>> callbacks;

        public TaskCallbackManager()
        {
            callbacks = new ConcurrentDictionary<int, TaskCompletionSource<object>>();
        }

        public Task<object> Create(int key)
        {
            var taskCompletionSource = callbacks.GetOrAdd(key, k => new TaskCompletionSource<object>());
            return taskCompletionSource.Task;
        }

        public bool Remove(int key)
        {
            TaskCompletionSource<object> callback;
            return callbacks.TryRemove(key, out callback);
        }

        public void SetResult(int key, object result)
        {
            TaskCompletionSource<object> callback;
            if (callbacks.TryRemove(key, out callback))
                callback.TrySetResult(result);
        }

        public void SetException(int key, Exception exception)
        {
            TaskCompletionSource<object> callback;
            if (callbacks.TryRemove(key, out callback))
                callback.TrySetException(exception);
        }

        public void SetResultForAll(object result)
        {
            var callbacks = this.callbacks.Select(x => x.Value).ToArray();
            this.callbacks.Clear();

            foreach (var callback in callbacks)
                callback.TrySetResult(result);
        }

        public void SetExceptionForAll(Exception exception)
        {
            var callbacks = this.callbacks.Select(x => x.Value).ToArray();
            this.callbacks.Clear();

            foreach (var callback in callbacks)
                callback.TrySetException(exception);
        }

        public void Clear()
        {
            callbacks.Clear();
        }
    }
}
