using System.Net.Mail;
using Aula.Server.Common.BackgroundTaskQueue;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Aula.Server.Common.Mail;

internal static class DependencyInjection
{
	internal static IServiceCollection AddMailSender(this IServiceCollection services)
	{
		_ = services.AddOptions<MailOptions>()
			.BindConfiguration(MailOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.TryAddTransient<SmtpClient>();
		services.TryAddSingleton<IEmailSender, DefaultEmailSender>();
		_ = services.AddBackgroundTaskQueueFor<IEmailSender>();
		return services;
	}
}
