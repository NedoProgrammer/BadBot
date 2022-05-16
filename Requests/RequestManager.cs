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
		public Stage(RequestManager manager)
		{
			LogAction = str =>
			{
				Log += str;
				manager.Update();
			};
			ProgressAction = i =>
			{
				ProgressBar.Value = i;
				manager.Update();
			};
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
	
	private Stage _downloading;
	private Stage _processing;
	private Stage _sending;
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
				var request = _handler.Request;

				if (Directory.GetDirectories("Requests").Length >= 5)
				{
					await _handler.RequestMessage.ModifyAsync(m => m.Content = "`warning: delaying request due to cache cleanup`", true);
					/*GC.Collect();
					GC.WaitForPendingFinalizers();*/
					foreach(var directory in Directory.GetDirectories("Requests"))
						Directory.Delete(directory, true);
				}

				_downloading = new(this);
				_processing = new(this);
				_sending = new(this);
				_current = null;

				try
				{
					Switch(_downloading);

					Emit("Creating request directory");
					Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Requests\\{request.Id}");
					_current!.ProgressBar.Value = 50;

					if (request.Source.PlatformSource == PlatformSource.Youtube && _handler.Video)
					{
						Emit("Using Youtube downloader");
						Emit($"Downloading video from {request.Source.Url}..");
						await FileDownloader.DownloadYoutube(request);
					}
					else
					{
						Emit("Using default downloader");
						Emit($"Downloading file from {request.Source.Url}..");
						await FileDownloader.DownloadSource(request);
					}

					Emit("Downloaded", "");
					_current!.ProgressBar.Value = 100;

					Switch(_processing);

					Emit("Calling processor method..");
					await _handler.Process();
					Emit("Finished", "");
					_current.ProgressBar.Value = 100;
					await UpdateStatus();

					Switch(_sending);

					Emit("Checking..");
					var resultFile = request.SourceDirectory() + $"\\result{request.Extension()}";
					if (!File.Exists(resultFile))
					{
						Emit("Result file does not exist!");
						await request.StatusMessage.ModifyAsync(m => m.Content = "а где..", true);
					}
					else if (new FileInfo(resultFile).Length > 8000000)
					{
						Emit("File too big!");
						await request.StatusMessage.ModifyAsync(
							m => m.Content = "а вот не могу я отправить!! файл большой слишкой..", true);
					}
					else
					{
						var stream = new FileStream(request.SourceDirectory() + $"\\result{request.Extension()}",
							FileMode.Open, FileAccess.Read);
						await request.Channel.SendMessageAsync(new DiscordMessageBuilder()
							.WithFile($"{request.Id}{request.Extension()}", stream)
							.WithContent(request.User.Mention)
							.WithAllowedMention(new UserMention(request.User)));
						stream.Close();
					}
				}
				catch (Exception e)
				{
					await request.StatusMessage.ModifyAsync(x =>
						x.Content = $"`failed to process source`\n```\n{e}\n{e.StackTrace}\n```");
				}

				//await request.StatusMessage.DeleteAsync();
				await _handler.RequestMessage.DeleteAsync();
				
				Log.Debug("Finished request with ID {Id}", request.Id);
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