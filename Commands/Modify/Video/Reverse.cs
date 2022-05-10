using BadBot.Commands;
using BadBot.Requests;
using DSharpPlus.SlashCommands;
namespace BadBot.Commands.Modify.Video;
public class Reverse: RequestCommandModule
{
	public Reverse()
	{
	}

	public override Task Process(Request request)
	{
		return Task.CompletedTask;
	}

	[SlashCommand("reverse", "развернуть видео")]
	public override async Task Execute(InteractionContext ctx, [Option("url", "ссылка на видео")] string url = "")
	{
		await AddToQueue(new RequestOptions(), ctx, url);
	}

	public override bool Video { get; protected set; } = true;
}