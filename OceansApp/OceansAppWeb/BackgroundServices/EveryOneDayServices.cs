using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Utility.Email;

namespace OceansAppWeb.BackgroundServices
{
    public class EveryOneDayServices: BackgroundService
    {
        private readonly ILogger<EveryOneDayServices> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EveryOneDayServices(ILogger<EveryOneDayServices> logger, IConfiguration config, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _config = config;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var costaRicaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                var currentTimeInCostaRica = TimeZoneInfo.ConvertTimeFromUtc(now, costaRicaZone);

                var nextRun = currentTimeInCostaRica.Date.AddHours(22).AddMinutes(35); // 9:00 AM from current day
                if (currentTimeInCostaRica > nextRun)
                {
                    nextRun = nextRun.AddDays(1); // If it has already passed 9:00 AM, schedule for the next day
                }

                var delay = nextRun - currentTimeInCostaRica;

                // Wait for the next execution
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var sendEmailRepository = scope.ServiceProvider.GetRequiredService<ISendEmailRepository>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        EjecutarTarea(sendEmailRepository, nextRun);
                    }
                }
            }
        }

        private void EjecutarTarea(ISendEmailRepository sendEmailRepository, DateTime nextRun)
        {
            // Lógica que quieres ejecutar periódicamente
            _logger.LogInformation("Ejecutando tarea a las {time}", DateTimeOffset.Now);
            var emailToSend = new SendEmailVM()
            {
                Subject = "Service executed automatically",
                Body = "The service was executes on: " + nextRun,
                EmailTo = "oscar.alfaro@oceanscode.com",
                SharedEmailFrom = _config["internalEmail"],
                EmailCcList = null
            };
            var emailSent = sendEmailRepository.SendEmail(emailToSend);
        }
    }
}
