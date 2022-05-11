using BadBot.Commands;
using BadBot.Helpers;
using BadBot.UI;
using DSharpPlus.Entities;
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
				_processingProgress = new TextProgressBar {Value = 0};
				_sendingProgress = new TextProgressBar {Value = 0};

				_handler.ProgressChanged += p => _processingProgress.Value = p;
				_handler.MessageEmitted += LogToDownload;

				Emit("Creating request directory");
				Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Requests\\{_handler.Request.Id}");
				_downloadProgress.Value = 50;

				switch (_handler.Request.Source.PlatformSource)
				{
					case PlatformSource.Youtube:
						Emit("Using Youtube downloader");
						break;
					case PlatformSource.Invalid:
						Emit("Using default downloader");
						Emit($"Downloading file from {_handler.Request.Source.Url}");
						await FileDownloader.DownloadSource(_handler.Request);
						Emit("Downloaded", "");
						_downloadProgress.Value = 100;
						await UpdateStatus();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				_handler.MessageEmitted -= LogToDownload;
				_handler.MessageEmitted += LogToProcess;
				
				Emit("Calling processor method");
				await _handler.Process();
				Emit("Finished", "");
				_processingProgress.Value = 100;
				await UpdateStatus();
			}	
		});
		Log.Information("Started request processor task");
	}

	private void LogToDownload(string log)
	{
		_downloadLog += log;
		Task.Run(async () => await UpdateStatus());
	}
	private void LogToProcess(string log)
	{
		_processingLog += log;
		Task.Run(async () => await UpdateStatus());
	}
	private void LogToSend(string log)
	{
		_sendingLog += log;
		Task.Run(async () => await UpdateStatus());
	}

	private void Emit(string str, string end = "\n") => _handler.Emit(str, end);

	private async Task UpdateStatus()
	{
		var embed = new DiscordEmbedBuilder()
			.WithTitle("статус")
			.WithColor(DiscordColor.Gold)
			.WithDescription($@"`ID: {_handler.Request.Id}`

**[1] Downloading source**
```
{_downloadLog}
{_downloadProgress}
```

**[2] Processing source**
```
{_processingLog}
{_processingProgress}
```

**[3] Sending source**
```
{_sendingLog}
{_sendingProgress}
```")
			.Build();
	
		await _handler.Request.StatusMessage.ModifyAsync("", embed);
	}
}