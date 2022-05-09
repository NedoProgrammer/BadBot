using Serilog.Core;
using Serilog.Events;

namespace BadBot.Extensions;

/// <summary>
/// Custom Console Serilog sink with an event
/// attached to when a message is emitted.
/// </summary>
public class EventSink : ILogEventSink
{
	/// <summary>
	/// Format provider.
	/// </summary>
	private readonly IFormatProvider _formatProvider;

	/// <summary>
	/// Constructor. Automatically empties the "latest_log.txt" file.
	/// </summary>
	/// <param name="formatProvider">Optional format provider.</param>
	public EventSink(IFormatProvider formatProvider)
	{
		_formatProvider = formatProvider;
		File.WriteAllText("latest_log.txt", "");
	}

	/// <summary>
	/// Derived from <see cref="ILogEventSink"/>.
	/// Writes to the file & console, also invokes the event. (if set)
	/// </summary>
	/// <param name="logEvent"></param>
	public void Emit(LogEvent logEvent)
	{
		var message = logEvent.RenderMessage(_formatProvider);
		var formatted = $"{DateTime.Now:g}:{DateTime.Now.Millisecond} [{logEvent.Level.ToString()}] {message}";
		#pragma warning disable Spectre1000
		Console.WriteLine(formatted);
		File.AppendAllText("latest_log.txt", formatted + "\n");
		#pragma warning restore Spectre1000
		Emitted?.Invoke();
	}

	/// <summary>
	/// Event which is invoked when a message is emitted.
	/// </summary>
	public event Action? Emitted;
}