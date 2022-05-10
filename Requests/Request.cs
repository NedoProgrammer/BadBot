using System.Reflection;
using DSharpPlus.Entities;
using Serilog;

namespace BadBot.Requests;

public class Request
{
	public static readonly Dictionary<Type, int> RequestTypes = new();

	public int Type;
	public string Id;
	public DiscordUser User;
	public DiscordChannel Channel;
	public DiscordMessage StatusMessage;
	public Source Source;

	public static void Register()
	{
		Log.Debug("Registering request types");
		var types = Assembly.GetExecutingAssembly().GetTypes()
			.Where(x => !string.IsNullOrEmpty(x.Namespace) && x.Namespace.Contains("BadBot.Commands.Modify")).ToList();
		for (var i = 0; i < types.Count; i++)
			RequestTypes[types[i]] = i;
		Log.Information("Registered {RequestTypeCount} request types", types.Count); }
}