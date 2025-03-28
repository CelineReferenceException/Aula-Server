﻿namespace Aula.Server.Common.Logging;

internal static class DependencyInjection
{
	internal static ILoggingBuilder AddLogging(this ILoggingBuilder logging)
	{
		logging.Services.TryAddSingleton<ILoggerProvider, DefaultLoggerProvider>();

		return logging;
	}
}
