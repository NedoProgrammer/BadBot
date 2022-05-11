using BadBot.Commands;
using BadBot.Extensions;
using BadBot.Requests;
using DSharpPlus.SlashCommands;
namespace BadBot.Commands.Modify.Video;
public class Reverse: RequestCommandModule
{
	public override async Task Process()
	{
		await this.ReverseSource();
	}

	[SlashCommand("reverse", "развернуть видео")]
	public override async Task Execute(InteractionContext ctx, [Option("url", "ссылка на видео")] string url = "")
	{
		await AddToQueue(new RequestOptions(), ctx, url);
	}

	public override bool Video { get; protected set; } = true;
}