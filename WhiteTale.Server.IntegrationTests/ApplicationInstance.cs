using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WhiteTale.Server.Common.Persistence;

namespace WhiteTale.Server.IntegrationTests;

internal sealed class ApplicationInstance : WebApplicationFactory<Program>
{
	private readonly String _contextName;

	internal ApplicationInstance(String contextName)
	{
		_contextName = contextName;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		_ = builder.ConfigureServices(services =>
		{
			_ = services.RemoveAll<IEmailSender>();
			_ = services.AddSingleton<IEmailSender, NoOpEmailSender>();

			_ = services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
			_ = services.AddSqlite<ApplicationDbContext>($"DataSource=./{_contextName}.db");
		});
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		var application = builder.Build();
		using var scope = application.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		_ = dbContext.Database.EnsureDeleted();
		_ = dbContext.Database.EnsureCreated();

		return builder.Start();
	}
}
