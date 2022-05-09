using BadBot.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace BadBot.Commands;

public class Ping: ApplicationCommandModule
{
	private static readonly string[] TitleMessages = {"хоб хоба инфу слили", "вычислили по ip..", "информация", "бип буп"};
	[SlashCommand("ping", "проверить, жив ли бот.")]
	public async Task ExecutePing(InteractionContext ctx)
	{
		var now = DateTime.Now;
		await ctx.CreateResponseAsync("секунду..");
		var difference = (DateTime.Now - now).Milliseconds;
		var title = RandomExtensions.New().NextElement(TitleMessages);
		var embed = new DiscordEmbedBuilder()
			.WithTitle(title)
			.AddField("задержка между сообщениями", difference + "мс")
			.AddField("задержка бота (gateway)", ctx.Client.Ping + "мс")
			.WithColor(DiscordColor.Gold)
			.Build();
		await ctx.EditResponseAsync(new DiscordWebhookBuilder()
			.AddEmbed(embed));
	}
}