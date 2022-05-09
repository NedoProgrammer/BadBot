using BadBot.Requests;
using DSharpPlus.SlashCommands;

namespace BadBot.Commands;

public abstract class RequestCommandModule: ApplicationCommandModule
{
	public abstract Task Process(Request request);
}