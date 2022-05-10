using BadBot.Commands;
using BadBot.UI;
using Serilog;

namespace BadBot.Requests;

public class RequestManager
{
	public static RequestManager Singleton = new();

	private RequestManager()
	{ }

	private Queue<RequestCommandModule> _handlers = new();
	public void AddRequest(RequestCommandModule module) => _handlers.Enqueue(module);

	private TextProgressBar _downloadProgress;
	private TextProgressBar _processingProgress;
	private TextProgressBar _sendingProgress;
	private string _downloadLog = "";
	private string _processingLog = "";
	private string _sendingLog = "";
	private RequestCommandModule _handler;
	public void StartProcessor()
	{
		Task.Run(async () =>
		{
			while (true)
			{
				while (_handlers.Count == 0) await Task.Delay(1000);
				_handler = _handlers.Dequeue();
				_downloadProgress = new TextProgressBar {Value = 0};
				_processingProgress = new TextProgressBar {Value = -1};
				_sendingProgress = new TextProgressBar {Value = -1};

				_handler.MessageEmitted += str => _downloadLog += str + "\n";
				await UpdateStatus();
			}
		});
		Log.Information("Started request processor task");
	}

	private void Emit(string str) => _handler.Emit(str);

	private async Task UpdateStatus()
	{
		var status = @$"```ansi
[0;40m[1;32m=== REQUEST {_handler.Request.Id} ===

{(_downloadProgress.Value == -1 ? "": $"[1] Downloading source\n{_downloadLog}\n{_downloadProgress}")}

{(_processingProgress.Value == -1 ? "": $"[1] Processing source\n{_processingLog}\n{_processingProgress}")}

{(_sendingProgress.Value == -1 ? "": $"[1] Downloading source\n{_sendingLog}\n{_sendingProgress}")}
```";
		await _handler.Request.StatusMessage.ModifyAsync(status);
	}
}