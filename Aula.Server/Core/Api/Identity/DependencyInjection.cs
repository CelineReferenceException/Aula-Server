﻿using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Core.Api.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentityApi(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options => options.SerializerOptions.TypeInfoResolverChain.Add(IdentityJsonContext.Default));
		_ = services.AddIdentityEndpointEmailSenders();
		return services;
	}

	private static IServiceCollection AddIdentityEndpointEmailSenders(this IServiceCollection services)
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
