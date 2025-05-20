using CarRentalSystem.Data;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Services
{
    public class EmailNotificationService : BackgroundService
    {
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EmailNotificationService(
            ILogger<EmailNotificationService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Notification Service is starting");

            // Run once on startup
            await SendNotifications(stoppingToken);

            // Then run periodically
            var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                await SendNotifications(stoppingToken);
            }
        }

        private async Task SendNotifications(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Checking for notifications to send");

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                // Send pickup reminders for bookings with pickup date tomorrow
                var pickupReminders = await dbContext.Bookings
                    .Where(b => b.Status == BookingStatus.Approved)
                    .Where(b => b.PickupDate.Date == tomorrow)
                    .Include(b => b.User)
                    .Include(b => b.Vehicle)
                    .ToListAsync(stoppingToken);

                foreach (var booking in pickupReminders)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    try
                    {
                        await emailService.SendPickupReminderAsync(booking, booking.User, booking.Vehicle);
                        _logger.LogInformation($"Sent pickup reminder for booking {booking.Id} to {booking.User.Email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending pickup reminder for booking {booking.Id}");
                    }
                }

                // Send return reminders for bookings with return date tomorrow
                var returnReminders = await dbContext.Bookings
                    .Where(b => b.Status == BookingStatus.Approved)
                    .Where(b => b.ReturnDate.Date == tomorrow)
                    .Include(b => b.User)
                    .Include(b => b.Vehicle)
                    .ToListAsync(stoppingToken);

                foreach (var booking in returnReminders)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    try
                    {
                        await emailService.SendReturnReminderAsync(booking, booking.User, booking.Vehicle);
                        _logger.LogInformation($"Sent return reminder for booking {booking.Id} to {booking.User.Email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending return reminder for booking {booking.Id}");
                    }
                }

                _logger.LogInformation($"Sent {pickupReminders.Count} pickup reminders and {returnReminders.Count} return reminders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification service");
            }
        }
    }
}