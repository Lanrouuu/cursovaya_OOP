using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Cursovaya.Services;

public class EmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _from;
    private readonly string _password;
    private readonly string _displayName;
    private readonly bool _isConfigured;

    public EmailService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Smtp");
        _host = section["Host"] ?? string.Empty;
        _port = int.TryParse(section["Port"], out var port) ? port : 587;
        _from = section["From"] ?? string.Empty;
        _password = section["Password"] ?? string.Empty;
        _displayName = section["DisplayName"] ?? "TradeAds";

        _isConfigured = !string.IsNullOrWhiteSpace(_host)
            && !string.IsNullOrWhiteSpace(_from)
            && !string.IsNullOrWhiteSpace(_password)
            && !_from.StartsWith("your-");
    }

    public async Task SendWelcomeAsync(string toEmail, string userName)
    {
        await SendAsync(toEmail,
            LocalizedStrings.Get("EmailWelcomeSubject"),
            LocalizedStrings.Format("EmailWelcomeBody", userName));
    }

    public async Task SendAccountBlockedAsync(string toEmail, string userName)
    {
        await SendAsync(toEmail,
            LocalizedStrings.Get("EmailAccountBlockedSubject"),
            LocalizedStrings.Format("EmailAccountBlockedBody", userName));
    }

    public async Task SendAccountUnblockedAsync(string toEmail, string userName)
    {
        await SendAsync(toEmail,
            LocalizedStrings.Get("EmailAccountUnblockedSubject"),
            LocalizedStrings.Format("EmailAccountUnblockedBody", userName));
    }

    public async Task SendAdvertisementBlockedAsync(string toEmail, string userName, string advertisementTitle)
    {
        await SendAsync(toEmail,
            LocalizedStrings.Get("EmailAdvertisementBlockedSubject"),
            LocalizedStrings.Format("EmailAdvertisementBlockedBody", userName, advertisementTitle));
    }

    public async Task SendAdvertisementActivatedAsync(string toEmail, string userName, string advertisementTitle)
    {
        await SendAsync(toEmail,
            LocalizedStrings.Get("EmailAdvertisementActivatedSubject"),
            LocalizedStrings.Format("EmailAdvertisementActivatedBody", userName, advertisementTitle));
    }

    public async Task SendExpiringWarningAsync(string toEmail, string userName, string advertisementTitle, int daysLeft)
    {
        await SendAsync(toEmail,
            LocalizedStrings.Get("EmailExpiringSubject"),
            LocalizedStrings.Format("EmailExpiringBody", userName, advertisementTitle, daysLeft));
    }

    private async Task SendAsync(string toEmail, string subject, string body)
    {
        if (!_isConfigured) return;

        try
        {
            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_from, _password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var from = new MailAddress(_from, _displayName);
            var to = new MailAddress(toEmail);

            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            await client.SendMailAsync(message);
        }
        catch
        {
            // Email failures are non-critical — silently swallow
        }
    }
}
