using BadBot.Drawing;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Serilog;

namespace BadBot.Core.EventHandlers;

public class SlashCommandErrored
{
	public static Task CommandsOnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
	{
		Log.Error("Failed to execute slash command ({CommandName})! Message: \"{Message}\"", e.Context.CommandName, e.Exception.ToString());
		Task.Run(async () =>
		{
			var image = await CrashedDrawer.Draw(sender.Client, e.Exception);
			image.Seek(0, SeekOrigin.Begin);
			await e.Context.Channel.SendMessageAsync(new DiscordMessageBuilder()
				.WithFile("Crashed.png", image));
		});
		return Task.CompletedTask;
	}
}