using Newtonsoft.Json;
using Spectre.Console;

namespace BadBot.Core;

/// <summary>
/// Config containing required variables required
/// for running the bot.
/// </summary>
public class Config
{
	/// <summary>
	/// Private singleton.
	/// </summary>
	private static Config? _config = null;

	/// <summary>
	/// Singleton.
	/// </summary>
	/// <exception cref="Exception">If the config was not yet loaded via Load().</exception>
	public static Config Singleton
	{
		get
		{
			if (_config == null)
				throw new Exception("Cannot get config while Load() was not called!");
			return _config;
		}
	}

	private Config()
	{
		
	}
	
	/// <summary>
	/// Discord bot token.
	/// </summary>
	public string Token = "DISCORD_BOT_TOKEN";

	/// <summary>
	/// If specified, will update slash commands immediately for this guild.
	/// </summary>
	public ulong GuildId = 0;

	/// <summary>
	/// Load the token from config.json.
	/// </summary>
	/// <exception cref="Exception">If the "config.json" file does not exist or the app failed to parse the config.</exception>
	public static void Load()
	{
		if (_config != null) AnsiConsole.MarkupLine("[yellow]Loading config twice is not recommended.[/]");
		
		if (!File.Exists("config.json"))
			throw new Exception("Cannot load config.json because it does not exist!");

		if (File.ReadAllText("config.json") == "{}")
		{
			AnsiConsole.MarkupLine("[yellow]It seems like you have an empty config. Please, edit it and restart the bot.[/]");
			File.WriteAllText("config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented));
			Environment.Exit(0);
		}
		
		_config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
	}

	public const ulong OwnerId = 492327234992865290;
}