using Microsoft.Extensions.Options;
using Zephyr.MetarUpdate;
using Microsoft.Extensions.DependencyInjection;

namespace Zephyr.Scheduler
{
    public class MetarBackgroundServce : BackgroundService
    {
        private readonly ILogger<MetarBackgroundServce> _logger;
        private readonly PeriodicTimer _timer;
        private readonly IServiceProvider _serviceProvider;

        public MetarBackgroundServce(ILogger<MetarBackgroundServce> logger,
            IOptions<BackgroundServiceOptions> options,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(options.Value.IntervalMinutes));
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var fetchRemoteMetar = scope.ServiceProvider.GetRequiredService<FetchRemoteMetar>();
                        await fetchRemoteMetar.UpdateMetarsAsync();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Background Service is stopping.");
            }
        }

    }
}
