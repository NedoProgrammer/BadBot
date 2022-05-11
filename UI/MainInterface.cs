using BadBot.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Serilog;
using Spectre.Console;

namespace BadBot.UI;

/// <summary>
/// The main screen the user sees while the bot is running.
/// </summary>
public class MainInterface
{
	private const int TrimAfter = 5;

	public static StringWriter ConsoleStream = new();
	public static EventSink Sink = new(null!);

	private static Tree _tree;
	
	private static TreeNode _logNode;
	private static Markup _logNotice;

	private static TreeNode _contextNode;

	private static LiveDisplayContext? _ctx;

	public static void Show()
	{
		AnsiConsole.Clear();
		Sink.Emitted += Refresh;

		_tree = new Tree("Bot");
		
		_logNode = _tree.AddNode("Log");
		_logNotice = new Markup("Check [yellow]latest_log.txt[/] for full log.");

		_contextNode = _tree.AddNode("Context");

		Task.Run(() => AnsiConsole.Live(_tree).AutoClear(false).Start(ctx =>
		{
			_ctx = ctx;
			while(true) Thread.Sleep(int.MaxValue);
			// ReSharper disable once FunctionNeverReturns
		}));
		Log.Debug("Started UI update task");
	}

	private static void Refresh()
	{
		_logNode.Nodes.Clear();
		var formattedText = string.Join("\n",
				ConsoleStream
					.ToString()
					.Trim()
					.Split("\n")
					.Select(x => x.Trim()).TakeLast(TrimAfter))
			.Replace("[", "[[").Replace("]", "]]");
		_logNode.AddNodes(new Panel(formattedText), _logNotice);
		
		_contextNode.Nodes.Clear();
		var contextText = _interactionContext == null ? "Waiting for commands..": $@"Command: /{_interactionContext.CommandName}
User: {_interactionContext.User.Id} ({_interactionContext.User.Username})
Channel: {_interactionContext.Channel.Id} (#{_interactionContext.Channel.Name})
Guild: {(_interactionContext.Channel.IsPrivate ? "no": $"{_interactionContext.Guild.Id} ({_interactionContext.Guild.Name})")}";
		_contextNode.AddNode(new Panel(contextText));

		try
		{
			_ctx?.Refresh();
		}
		catch(Exception e)
		{
			AnsiConsole.WriteLine("Failed to update context");
			AnsiConsole.WriteException(e);
		}
	}

	private static InteractionContext? _interactionContext;
	public static Task OnSlashCommandInvoked(SlashCommandsExtension sender, SlashCommandInvokedEventArgs e)
	{
		_interactionContext = e.Context;
		Refresh();
		return Task.CompletedTask;
	}
	
	public static Task OnSlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
	{
		_interactionContext = null;
		Refresh();
		return Task.CompletedTask;
	}
}