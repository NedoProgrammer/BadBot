using System.Reflection;
using BadBot.Core.EventHandlers;
using BadBot.Drawing;
using BadBot.UI;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;

namespace BadBot.Core;

/// <summary>
/// The Discord client wrapper.
/// </summary>
public class Bot
{
	/// <summary>
	/// Singleton.
	/// </summary>
	public static Bot Singleton { get; } = new();
	
	/// <summary>
	/// Private constructor.
	/// </summary>
	private Bot()
	{
	}

	/// <summary>
	/// Discord client.
	/// </summary>
	private DiscordClient _client;

	/// <summary>
	/// Start the bot (sync).
	/// </summary>
	public void Start()
	{
		StartAsync().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Start the bot (async).
	/// </summary>
	private async Task StartAsync()
	{
		//Initialize client
		var factory = new LoggerFactory().AddSerilog();
		_client = new DiscordClient(new DiscordConfiguration
		{
			Intents = DiscordIntents.All,
			Token = Config.Singleton.Token,
			TokenType = TokenType.Bot,
			LoggerFactory = factory,
			#if DEBUG
			MinimumLogLevel = LogLevel.Debug
			#else
			MinimumLogLevel = LogLevel.Information
			#endif
		});
		Log.Information("Client created");

		//Attach event handlers
		_client.Ready += (_, _) =>
		{
			Log.Information("Bot (re)connected");
			return Task.CompletedTask;
		};

		//Enable slash commands
		var commands = _client.UseSlashCommands();
		commands.RegisterCommands(Assembly.GetExecutingAssembly(), Config.Singleton.GuildId == 0 ? null: Config.Singleton.GuildId);
		
		commands.SlashCommandErrored += SlashCommandErrored.CommandsOnSlashCommandErrored;
		commands.SlashCommandInvoked += MainInterface.OnSlashCommandInvoked;
		commands.SlashCommandExecuted += MainInterface.OnSlashCommandExecuted;
		
		Log.Debug("Enabled {SlashCommandCount} slash commands", commands.RegisteredCommands.Count);
		
		//Enable interactivity
		_client.UseInteractivity();
		Log.Debug("Enabled interactivity");

		//Login!
		await _client.ConnectAsync();
		await Task.Delay(-1);
	}
}