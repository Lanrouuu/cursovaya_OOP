using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Diagnostics;

namespace Cursovaya.Services;

public class EmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _from;
    private readonly string _password;
    private readonly string _displayName;
    private readonly bool _enableSsl;
    private readonly bool _isConfigured;

    public EmailService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Smtp");
        _host = section["Host"] ?? string.Empty;
        _port = int.TryParse(section["Port"], out var port) ? port : 587;
        _from = (section["From"] ?? string.Empty).Trim();
        _password = (section["Password"] ?? string.Empty).Replace(" ", string.Empty);
        _displayName = section["DisplayName"] ?? "TradeAds";
        _enableSsl = !bool.TryParse(section["EnableSsl"], out var enableSsl) || enableSsl;

        _isConfigured = IsRealCredential(_host)
            && IsRealCredential(_from)
            && IsRealCredential(_password);
    }

    public bool IsConfigured => _isConfigured;

    public string? LastError { get; private set; }

    public Task<bool> SendWelcomeAsync(string toEmail, string userName) =>
        TrySendAsync(toEmail,
            LocalizedStrings.Get("EmailWelcomeSubject"),
            LocalizedStrings.Format("EmailWelcomeBody", userName));

    public Task SendAccountBlockedAsync(string toEmail, string userName) =>
        TrySendAsync(toEmail,
            LocalizedStrings.Get("EmailAccountBlockedSubject"),
            LocalizedStrings.Format("EmailAccountBlockedBody", userName));

    public Task SendAccountUnblockedAsync(string toEmail, string userName) =>
        TrySendAsync(toEmail,
            LocalizedStrings.Get("EmailAccountUnblockedSubject"),
            LocalizedStrings.Format("EmailAccountUnblockedBody", userName));

    public Task SendAdvertisementBlockedAsync(string toEmail, string userName, string advertisementTitle) =>
        TrySendAsync(toEmail,
            LocalizedStrings.Get("EmailAdvertisementBlockedSubject"),
            LocalizedStrings.Format("EmailAdvertisementBlockedBody", userName, advertisementTitle));

    public Task SendAdvertisementActivatedAsync(string toEmail, string userName, string advertisementTitle) =>
        TrySendAsync(toEmail,
            LocalizedStrings.Get("EmailAdvertisementActivatedSubject"),
            LocalizedStrings.Format("EmailAdvertisementActivatedBody", userName, advertisementTitle));

    public Task SendExpiringWarningAsync(string toEmail, string userName, string advertisementTitle, int daysLeft) =>
        TrySendAsync(toEmail,
            LocalizedStrings.Get("EmailExpiringSubject"),
            LocalizedStrings.Format("EmailExpiringBody", userName, advertisementTitle, daysLeft));

    public async Task<bool> TrySendAsync(string toEmail, string subject, string body)
    {
        LastError = null;

        if (!_isConfigured || string.IsNullOrWhiteSpace(toEmail))
        {
            LastError = "SMTP is not configured.";
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_displayName, _from));
            message.To.Add(MailboxAddress.Parse(toEmail.Trim()));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            var secureSocketOptions = GetSecureSocketOptions();
            await client.ConnectAsync(_host, _port, secureSocketOptions);
            await client.AuthenticateAsync(_from, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            Trace.TraceWarning("SMTP send failed to {0}: {1}", toEmail, ex.Message);
            return false;
        }
    }

    private SecureSocketOptions GetSecureSocketOptions()
    {
        if (!_enableSsl)
        {
            return SecureSocketOptions.None;
        }

        return _port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
    }

    private static bool IsRealCredential(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        return !trimmed.StartsWith("your-", StringComparison.OrdinalIgnoreCase)
            && !trimmed.Equals("your-app-password", StringComparison.OrdinalIgnoreCase)
            && !trimmed.Contains("REPLACE", StringComparison.OrdinalIgnoreCase)
            && !trimmed.Contains("you@gmail.com", StringComparison.OrdinalIgnoreCase)
            && !trimmed.Contains("xxxx", StringComparison.OrdinalIgnoreCase);
    }
}
