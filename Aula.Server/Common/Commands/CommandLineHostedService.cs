using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aula.Server.Common.Commands;

/// <summary>
///     A background service that asynchronously reads from the console input stream and executes commands.
/// </summary>
internal sealed class CommandLineHostedService : BackgroundService
{
	private readonly CommandLine _commandLine;
	private readonly ILogger<CommandLineHostedService> _logger;

	public CommandLineHostedService(CommandLine commandLine, ILogger<CommandLineHostedService> logger)
	{
		_commandLine = commandLine;
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
				_ = await _commandLine.ProcessCommandAsync(line.AsMemory(), stoppingToken);
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
