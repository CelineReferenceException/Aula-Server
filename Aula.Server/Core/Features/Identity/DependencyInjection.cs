namespace Aula.Server.Core.Features.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentityEndpointEmailSenders(this IServiceCollection services)
	{
		_ = services.AddOptions<IdentityFeatureOptions>()
			.BindConfiguration(IdentityFeatureOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.AddScoped<ResetPasswordEmailSender>();
		_ = services.AddScoped<ConfirmEmailEmailSender>();

		return services;
	}
}
