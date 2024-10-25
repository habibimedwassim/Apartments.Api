using Apartments.Application.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.BackgroundServices;
public class RentTransactionScheduler(
    ILogger<RentTransactionScheduler> logger, 
    IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Console.WriteLine(today.AddDays(4));
        logger.LogInformation("Rent Transaction Scheduler is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var targetTime = new DateTime(now.Year, now.Month, now.Day, 06, 00, 0, DateTimeKind.Utc);
            var delay = targetTime - now;

            // If the target time has already passed today, schedule for the next day
            if (delay < TimeSpan.Zero)
            {
                delay = delay.Add(TimeSpan.FromDays(1));
            }

            logger.LogInformation($"Next run scheduled for: {targetTime}");

            // Wait until the scheduled time
            await Task.Delay(delay, stoppingToken);

            // Execute the daily job
            await CreateRentTransactionsIfNeeded(stoppingToken);
        }

        logger.LogInformation("Rent Transaction Scheduler is stopping.");
    }

    private async Task CreateRentTransactionsIfNeeded(CancellationToken stoppingToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var rentTransactionService = scope.ServiceProvider.GetRequiredService<IRentTransactionService>();

            logger.LogInformation("Running daily rent transaction check...");

            try
            {
                await rentTransactionService.CheckAndCreateUpcomingRentTransactionsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking and creating rent transactions.");
            }
        }
    }
}
