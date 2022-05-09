using BadBot.Core;
using BadBot.Drawing;
using BadBot.Requests;
using BadBot.UI;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<DefaultCommand>();
app.Configure(cfg =>
{
	cfg.SetExceptionHandler(ex => AnsiConsole.WriteException(ex, ExceptionFormats.Default | ExceptionFormats.ShowLinks));
});
app.Run(args);

/// <summary>
/// The default run command for the bot.
/// </summary>
public class DefaultCommand : Command
{
	/// <summary>
	/// Start the bot.
	/// </summary>
	/// <param name="context">Not used.</param>
	/// <returns>Exit code.</returns>
	public override int Execute(CommandContext context)
	{
		InitializeLogger();
		
		ResourceChecker.Show();
		
		Config.Load();
		Request.Register();
		FontManager.Init();

		MainInterface.Show();
		Bot.Singleton.Start();
		
		return 0;
	}

	/// <summary>
	/// Initialize the Serilog logger.
	/// </summary>
	private void InitializeLogger()
	{
		AnsiConsole.MarkupLine("[yellow]Redirecting console output..[/]");
		Console.SetOut(MainInterface.ConsoleStream);
		
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Sink(MainInterface.Sink)
			#if DEBUG
			.MinimumLevel.Debug()
			#else
			.MinimumLevel.Information()
			#endif
			.CreateLogger();
		Log.Information("Logger initialized");
	}
}