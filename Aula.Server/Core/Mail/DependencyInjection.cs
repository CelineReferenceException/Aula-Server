using System.Net.Mail;
using Aula.Server.Core.BackgroundTaskQueue;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Core.Mail;

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
