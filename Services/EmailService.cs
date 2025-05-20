using CarRentalSystem.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CarRentalSystem.Services
{
    public interface IEmailService
    {
        Task SendBookingConfirmationAsync(Booking booking, User user, Vehicle vehicle);
        Task SendBookingApprovalAsync(Booking booking, User user, Vehicle vehicle);
        Task SendBookingCancellationAsync(Booking booking, User user, Vehicle vehicle);
        Task SendPickupReminderAsync(Booking booking, User user, Vehicle vehicle);
        Task SendReturnReminderAsync(Booking booking, User user, Vehicle vehicle);
        Task SendDamageReportAsync(Booking booking, User user, Vehicle vehicle, DamageReport damageReport);
        Task SendWelcomeEmailAsync(User user);
        Task SendPasswordResetAsync(User user, string resetToken);
        Task SendTestEmailAsync(string toEmail);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enabled;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _smtpServer = _configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:SmtpUsername"];
            _smtpPassword = _configuration["Email:SmtpPassword"];
            _fromEmail = _configuration["Email:FromEmail"];
            _fromName = _configuration["Email:FromName"] ?? "Car Rental System";
            _enabled = bool.Parse(_configuration["Email:Enabled"] ?? "false");
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            // If email is disabled in configuration, just log the email
            if (!_enabled)
            {
                _logger.LogInformation($"Email sending is disabled. Would have sent to {to}, Subject: {subject}");
                _logger.LogDebug($"Email body: {htmlBody}");
                return;
            }

            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(to));

                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}, Subject: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}, Subject: {subject}");
                throw;
            }
        }

        public async Task SendBookingConfirmationAsync(Booking booking, User user, Vehicle vehicle)
        {
            var subject = $"Booking Confirmation - #{booking.Id}";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine($"<h1>Booking Confirmation - #{booking.Id}</h1>");
            body.AppendLine($"<p>Dear {user.FirstName} {user.LastName},</p>");
            body.AppendLine("<p>Thank you for your booking with our car rental service. Your booking has been received and is awaiting approval.</p>");
            body.AppendLine("<h2>Booking Details:</h2>");
            body.AppendLine("<ul>");
            body.AppendLine($"<li><strong>Vehicle:</strong> {vehicle.Make} {vehicle.Model} ({vehicle.Year})</li>");
            body.AppendLine($"<li><strong>Pickup Date:</strong> {booking.PickupDate:dddd, MMMM d, yyyy}</li>");
            body.AppendLine($"<li><strong>Return Date:</strong> {booking.ReturnDate:dddd, MMMM d, yyyy}</li>");
            body.AppendLine($"<li><strong>Pickup Location:</strong> {booking.PickupLocation}</li>");
            body.AppendLine($"<li><strong>Return Location:</strong> {booking.ReturnLocation}</li>");
            body.AppendLine($"<li><strong>Total Price:</strong> ${booking.TotalPrice:F2}</li>");
            body.AppendLine($"<li><strong>Payment Method:</strong> {booking.PaymentMethod}</li>");
            body.AppendLine("</ul>");
            body.AppendLine("<p>You will receive another email once your booking has been approved.</p>");
            body.AppendLine("<p>Thank you for choosing our service!</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(user.Email, subject, body.ToString());
        }

        public async Task SendBookingApprovalAsync(Booking booking, User user, Vehicle vehicle)
        {
            var subject = $"Booking Approved - #{booking.Id}";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine($"<h1>Booking Approved - #{booking.Id}</h1>");
            body.AppendLine($"<p>Dear {user.FirstName} {user.LastName},</p>");
            body.AppendLine("<p>We're happy to inform you that your booking has been approved!</p>");
            body.AppendLine("<h2>Booking Details:</h2>");
            body.AppendLine("<ul>");
            body.AppendLine($"<li><strong>Vehicle:</strong> {vehicle.Make} {vehicle.Model} ({vehicle.Year})</li>");
            body.AppendLine($"<li><strong>Pickup Date:</strong> {booking.PickupDate:dddd, MMMM d, yyyy}</li>");
            body.AppendLine($"<li><strong>Return Date:</strong> {booking.ReturnDate:dddd, MMMM d, yyyy}</li>");
            body.AppendLine($"<li><strong>Pickup Location:</strong> {booking.PickupLocation}</li>");
            body.AppendLine($"<li><strong>Return Location:</strong> {booking.ReturnLocation}</li>");
            body.AppendLine($"<li><strong>Total Price:</strong> ${booking.TotalPrice:F2}</li>");
            body.AppendLine("</ul>");
            body.AppendLine("<h3>Pickup Instructions:</h3>");
            body.AppendLine("<p>Please bring your driver's license and ID when picking up the vehicle. If you're paying at pickup, please also bring a valid payment method.</p>");
            body.AppendLine("<p>Thank you for choosing our service!</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(user.Email, subject, body.ToString());
        }

        public async Task SendBookingCancellationAsync(Booking booking, User user, Vehicle vehicle)
        {
            var subject = $"Booking Cancelled - #{booking.Id}";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine($"<h1>Booking Cancelled - #{booking.Id}</h1>");
            body.AppendLine($"<p>Dear {user.FirstName} {user.LastName},</p>");
            body.AppendLine("<p>Your booking has been cancelled as requested.</p>");
            body.AppendLine("<h2>Cancelled Booking Details:</h2>");
            body.AppendLine("<ul>");
            body.AppendLine($"<li><strong>Vehicle:</strong> {vehicle.Make} {vehicle.Model} ({vehicle.Year})</li>");
            body.AppendLine($"<li><strong>Pickup Date:</strong> {booking.PickupDate:dddd, MMMM d, yyyy}</li>");
            body.AppendLine($"<li><strong>Return Date:</strong> {booking.ReturnDate:dddd, MMMM d, yyyy}</li>");
            body.AppendLine("</ul>");

            if (booking.PaymentStatus == PaymentStatus.Refunded)
            {
                body.AppendLine("<p>Your payment has been refunded. The refund may take 3-5 business days to appear on your statement.</p>");
            }

            body.AppendLine("<p>We hope to serve you again soon!</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(user.Email, subject, body.ToString());
        }

        public async Task SendPickupReminderAsync(Booking booking, User user, Vehicle vehicle)
        {
            var subject = $"Pickup Reminder - {vehicle.Make} {vehicle.Model}";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine($"<h1>Your {vehicle.Make} {vehicle.Model} is ready for pickup tomorrow!</h1>");
            body.AppendLine($"<p>Dear {user.FirstName} {user.LastName},</p>");
            body.AppendLine("<p>This is a friendly reminder that your car rental begins tomorrow.</p>");
            body.AppendLine("<h2>Pickup Details:</h2>");
            body.AppendLine("<ul>");
            body.AppendLine($"<li><strong>Vehicle:</strong> {vehicle.Make} {vehicle.Model} ({vehicle.Year})</li>");
            body.AppendLine($"<li><strong>Pickup Date:</strong> {booking.PickupDate:dddd, MMMM d, yyyy}</li>");
            body.AppendLine($"<li><strong>Pickup Location:</strong> {booking.PickupLocation}</li>");
            body.AppendLine("</ul>");
            body.AppendLine("<h3>What to Bring:</h3>");
            body.AppendLine("<ul>");
            body.AppendLine("<li>Driver's License</li>");
            body.AppendLine("<li>ID Card or Passport</li>");
            if (booking.PaymentMethod == PaymentMethod.PayAtPickup)
            {
                body.AppendLine("<li>Payment Method (Credit/Debit Card)</li>");
            }
            body.AppendLine("</ul>");
            body.AppendLine("<p>We look forward to serving you!</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(user.Email, subject, body.ToString());
        }

        public async Task SendReturnReminderAsync(Booking booking, User user, Vehicle vehicle)
        {
            var subject = $"Return Reminder - {vehicle.Make} {vehicle.Model}";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine($"<h1>Reminder: Your {vehicle.Make} {vehicle.Model} is due for return tomorrow</h1>");
            body.AppendLine($"<p>Dear {user.FirstName} {user.LastName},</p>");
            body.AppendLine("<p>This is a friendly reminder that your car rental period is ending tomorrow.</p>");
            body.AppendLine("<h2>Return Details:</h2>");
            body.AppendLine("<ul>");
            body.AppendLine($"<li><strong>Vehicle:</strong> {vehicle.Make} {vehicle.Model} ({vehicle.Year})</li>");
            body.AppendLine($"<li><strong>Return Date:</strong> {booking.ReturnDate:dddd, MMMM d, yyyy}</li>");
            body.AppendLine($"<li><strong>Return Location:</strong> {booking.ReturnLocation}</li>");
            body.AppendLine("</ul>");
            body.AppendLine("<h3>Return Instructions:</h3>");
            body.AppendLine("<ul>");
            body.AppendLine("<li>Please ensure the fuel tank is full</li>");
            body.AppendLine("<li>Remove all personal belongings</li>");
            body.AppendLine("<li>Bring the car and keys to the return location</li>");
            body.AppendLine("</ul>");
            body.AppendLine("<p>If you need to extend your rental, please log in to your account or contact us as soon as possible.</p>");
            body.AppendLine("<p>Thank you for choosing our service!</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(user.Email, subject, body.ToString());
        }

        public async Task SendDamageReportAsync(Booking booking, User user, Vehicle vehicle, DamageReport damageReport)
        {
            var subject = $"Damage Report - Booking #{booking.Id}";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine($"<h1>Damage Report - Booking #{booking.Id}</h1>");
            body.AppendLine($"<p>Dear {user.FirstName} {user.LastName},</p>");
            body.AppendLine("<p>We are writing to inform you about damages found on the vehicle you recently rented.</p>");
            body.AppendLine("<h2>Booking Details:</h2>");
            body.AppendLine("<ul>");
            body.AppendLine($"<li><strong>Vehicle:</strong> {vehicle.Make} {vehicle.Model} ({vehicle.Year})</li>");
            body.AppendLine($"<li><strong>Rental Period:</strong> {booking.PickupDate:MMM d, yyyy} - {booking.ReturnDate:MMM d, yyyy}</li>");
            body.AppendLine("</ul>");
            body.AppendLine("<h2>Damage Details:</h2>");
            body.AppendLine("<ul>");
            body.AppendLine($"<li><strong>Description:</strong> {damageReport.Description}</li>");
            body.AppendLine($"<li><strong>Repair Cost:</strong> ${damageReport.RepairCost:F2}</li>");
            body.AppendLine($"<li><strong>Customer Responsible:</strong> {(damageReport.IsCustomerResponsible ? "Yes" : "No")}</li>");
            body.AppendLine("</ul>");

            if (damageReport.IsCustomerResponsible)
            {
                body.AppendLine("<p>As per our terms and conditions, you are responsible for the damages described above. The repair cost will be charged to the payment method associated with your booking.</p>");
            }
            else
            {
                body.AppendLine("<p>After reviewing the circumstances, we have determined that these damages are not your responsibility. No additional charges will be applied.</p>");
            }

            body.AppendLine("<p>If you have any questions or concerns regarding this report, please contact our customer service.</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(user.Email, subject, body.ToString());
        }

        public async Task SendWelcomeEmailAsync(User user)
        {
            var subject = "Welcome to Our Car Rental Service";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine("<h1>Welcome to Our Car Rental Service!</h1>");
            body.AppendLine($"<p>Dear {user.FirstName} {user.LastName},</p>");
            body.AppendLine("<p>Thank you for registering with our car rental service. We're excited to have you as a member!</p>");
            body.AppendLine("<h2>Your Account is Ready</h2>");
            body.AppendLine("<p>You can now log in to your account and start exploring our wide selection of vehicles for rent.</p>");
            body.AppendLine("<h3>Next Steps:</h3>");
            body.AppendLine("<ol>");
            body.AppendLine("<li>Complete your profile by uploading your driver's license and ID</li>");
            body.AppendLine("<li>Browse available vehicles</li>");
            body.AppendLine("<li>Make your first booking</li>");
            body.AppendLine("</ol>");
            body.AppendLine("<p>If you have any questions, please don't hesitate to contact our customer service.</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(user.Email, subject, body.ToString());
        }

        public async Task SendPasswordResetAsync(User user, string resetToken)
        {
            var subject = "Password Reset Request";

            var resetUrl = $"{_configuration["AppUrl"]}/reset-password?token={resetToken}&email={System.Web.HttpUtility.UrlEncode(user.Email)}";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine("<h1>Password Reset Request</h1>");
            body.AppendLine($"<p>Dear {user.FirstName} {user.LastName},</p>");
            body.AppendLine("<p>We received a request to reset your password. If you didn't make this request, you can safely ignore this email.</p>");
            body.AppendLine("<p>To reset your password, click the link below:</p>");
            body.AppendLine($"<p><a href=\"{resetUrl}\">Reset Your Password</a></p>");
            body.AppendLine("<p>This link will expire in 24 hours.</p>");
            body.AppendLine("<p>If the button above doesn't work, copy and paste the following URL into your browser:</p>");
            body.AppendLine($"<p>{resetUrl}</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(user.Email, subject, body.ToString());
        }

        public async Task SendTestEmailAsync(string toEmail)
        {
            var subject = "Test Email from Car Rental System";

            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine("<h1>Test Email</h1>");
            body.AppendLine("<p>This is a test email from the Car Rental System.</p>");
            body.AppendLine($"<p>It was sent at: {DateTime.UtcNow}</p>");
            body.AppendLine("<p>If you received this email, the email service is working correctly.</p>");
            body.AppendLine("<p>Best regards,<br>The Car Rental Team</p>");
            body.AppendLine("</body></html>");

            await SendEmailAsync(toEmail, subject, body.ToString());
        }
    }
}