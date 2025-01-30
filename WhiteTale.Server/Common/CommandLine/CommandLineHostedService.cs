using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace WhiteTale.Server.Common.CommandLine;

internal sealed class CommandLineHostedService : BackgroundService
{
	private readonly CommandLineService _commandLineService;
	private readonly ILogger<CommandLineHostedService> _logger;

	public CommandLineHostedService(CommandLineService commandLineService, ILogger<CommandLineHostedService> logger)
	{
		_commandLineService = commandLineService;
		_logger = logger;
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Console.In.ReadLineAsync is blocking, so we use the standard input stream directly and read it asynchronously.
		using var inputReader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
		while (await inputReader.ReadLineAsync(stoppingToken) is { } line)
		{
			try
			{
				_ = await _commandLineService.ProcessCommandAsync(line.AsMemory(), stoppingToken);
			}
			catch (Exception ex)
			{
				// We catch the exception and do not rethrow it, as that would stop the process
				// and prevent the commandline service from processing further inputs.
				_logger.CommandFailed(ex);
			}
		}
	}
}
