
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.DataAccess.BackgroundServices
{
    public class BackgroundTaskService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BackgroundTaskService(IBackgroundTaskQueue taskQueue, IServiceScopeFactory serviceScopeFactory)
        {
            _taskQueue = taskQueue;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(_serviceScopeFactory, stoppingToken);
                }
                catch (Exception exception)
                {
                    // Here you should handle any exceptions that occurred in the task
                    // For example, log the error
                }
            }
        }
    }

}
