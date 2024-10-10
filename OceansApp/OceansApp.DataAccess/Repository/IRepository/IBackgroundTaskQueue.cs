
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<IServiceScopeFactory, CancellationToken, Task> workItem);
        Task<Func<IServiceScopeFactory, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private ConcurrentQueue<Func<IServiceScopeFactory, CancellationToken, Task>> _workItems =
            new ConcurrentQueue<Func<IServiceScopeFactory, CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(
            Func<IServiceScopeFactory, CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<IServiceScopeFactory, CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            if (_workItems.TryDequeue(out var workItem))
            {
                return workItem;
            }
            else
            {
                throw new InvalidOperationException("Failed to dequeue a work item.");
            }
        }
    }
}
