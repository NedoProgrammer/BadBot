using System.Reflection;
using Serilog;

namespace BadBot.Requests;

public class Request
{
	private static Dictionary<int, string> _requestTypes = new();

	public static void Register()
	{
		Log.Debug("Registering request types");
		var types = Assembly.GetExecutingAssembly().GetTypes()
			.Where(x => !string.IsNullOrEmpty(x.Namespace) && x.Namespace.Contains("BadBot.Commands.Modify")).ToList();
		for (var i = 0; i < types.Count; i++)
			_requestTypes[i] = types[0].Name;
		Log.Information("Registered {RequestTypeCount} request types", types.Count);
	}
}