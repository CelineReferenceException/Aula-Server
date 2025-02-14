using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace Aula.Server.Common.Mail;

internal sealed class DefaultEmailSender : IEmailSender
{
	private readonly MailAddress _mailAddress;
	private readonly SmtpClient _smtpClient;
	private readonly IBackgroundTaskQueue<IEmailSender> _taskQueue;

	public DefaultEmailSender(
		SmtpClient smtpClient,
		IOptions<ApplicationOptions> applicationOptions,
		IOptions<MailOptions> mailOptions,
		IBackgroundTaskQueue<IEmailSender> taskQueue)
	{
		_taskQueue = taskQueue;

		var applicationName = applicationOptions.Value.Name;
		var address = mailOptions.Value.Address;
		var password = mailOptions.Value.Password;
		var smtpHost = mailOptions.Value.SmtpHost;
		var smtpPort = mailOptions.Value.SmtpPort.Value;
		var enableSsl = mailOptions.Value.EnableSsl.Value;

		_mailAddress = new MailAddress(address, applicationName);

		smtpClient.Host = smtpHost;
		smtpClient.Port = smtpPort;
		smtpClient.Credentials = new NetworkCredential(address, password);
		smtpClient.EnableSsl = enableSsl;
		_smtpClient = smtpClient;
	}

	public async Task SendEmailAsync(String email, String subject, String htmlMessage)
	{
		await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
		{
			using var message = new MailMessage();
			message.From = _mailAddress;
			message.To.Add(email);
			message.Subject = subject;
			message.Body = htmlMessage;
			message.IsBodyHtml = true;

			await _smtpClient.SendMailAsync(message, ct);
		});
	}
}
