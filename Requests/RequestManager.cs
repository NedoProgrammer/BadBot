using System.Runtime.CompilerServices;
using BadBot.Commands;
using BadBot.Extensions;
using BadBot.Helpers;
using BadBot.UI;
using DSharpPlus.Entities;
using Serilog;

namespace BadBot.Requests;

public class RequestManager
{
	private class Stage
	{
		public Action<string> LogAction;
		public Action<int> ProgressAction;

		public Stage()
		{
			LogAction = str => Log += str;
			ProgressAction = i => ProgressBar.Value = i;
		}

		public string Log { get; private set; }
		public TextProgressBar ProgressBar = new() {Value = 0};

		public override string ToString() => $"```\n{Log}\n{ProgressBar}\n```";
	}

	public static RequestManager Singleton = new();

	private RequestManager()
	{ }

	private Queue<RequestCommandModule> _handlers = new();
	public void AddRequest(RequestCommandModule module) => _handlers.Enqueue(module);
	
	private Stage _downloading = new();
	private Stage _processing = new();
	private Stage _sending = new();
	private RequestCommandModule _handler;
	private Stage? _current;
	public void StartProcessor()
	{
		Task.Run(async () =>
		{
			while (true)
			{
				while (_handlers.Count == 0) await Task.Delay(1000);
				_handler = _handlers.Dequeue();
				var request = _handler;

				_downloading = new();
				_processing = new();
				_sending = new();
				_current = null;

				Switch(_downloading);

				Emit("Creating request directory");
				Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Requests\\{_handler.Request.Id}");
				_current!.ProgressBar.Value = 50;

				switch (_handler.Request.Source.PlatformSource)
				{
					case PlatformSource.Youtube:
						Emit("Using Youtube downloader");
						break;
					case PlatformSource.Invalid:
						Emit("Using default downloader");
						Emit($"Downloading file from {_handler.Request.Source.Url}..");
						await FileDownloader.DownloadSource(_handler.Request);
						Emit("Downloaded", "");
						_current!.ProgressBar.Value = 100;
						await UpdateStatus();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				Switch(_processing);
				
				Emit("Calling processor method..");
				await _handler.Process();
				Emit("Finished", "");
				_current.ProgressBar.Value = 100;
				await UpdateStatus();

				Switch(_sending);
				
				Emit("Checking..");
				if (new FileInfo(_handler.Request.SourceDirectory() + $"\\result{_handler.Request.Extension()}")
					    .Length > 8000000)
				{
					Emit("File too big!");
					await _handler.Request.StatusMessage.ModifyAsync("а вот не могу я отправить!! файл большой слишком..", Optional.FromNoValue<DiscordEmbed>());
					return;
				}

				var stream = new FileStream(_handler.Request.SourceDirectory() + $"\\result{_handler.Request.Extension()}", FileMode.Open, FileAccess.Read);
				await _handler.Request.Channel.SendMessageAsync(new DiscordMessageBuilder()
					.WithFile(Path.GetFileName(_handler.Request.SourceFullPath()), stream)
					.WithContent(_handler.Request.User.Mention));
				stream.Close();
				await _handler.Request.StatusMessage.DeleteAsync();
				await _handler.RequestMessage.DeleteAsync();
			}	
		});
		Log.Information("Started request processor task");
	}

	private void Switch(Stage stage)
	{
		_handler.MessageEmitted -= _current?.LogAction;
		_handler.ProgressChanged -= _current?.ProgressAction;

		_current = stage;
		
		_handler.MessageEmitted += _current.LogAction;
		_handler.ProgressChanged += _current.ProgressAction;
	}

	private int _count;
	private void Update()
	{
		if (_count == 3)
		{
			_count = 0;
			Task.Run(async () => await UpdateStatus());
		}

		_count++;
	}

	private void Emit(string str, string end = "\n") => _handler.Emit(str, end);

	private async Task UpdateStatus()
	{
		var embed = new DiscordEmbedBuilder()
			.WithTitle("статус")
			.WithColor(DiscordColor.Gold)
			.WithDescription($@"`ID: {_handler.Request.Id}`

**[1] Downloading source**
{_downloading}

**[2] Processing source**
{_processing}

**[3] Sending source**
{_sending}")
			.Build();
	
		await _handler.Request.StatusMessage.ModifyAsync("", embed);
	}
}