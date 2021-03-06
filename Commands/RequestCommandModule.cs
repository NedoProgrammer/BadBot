using System.Text.RegularExpressions;
using BadBot.Helpers;
using BadBot.Requests;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Serilog;

namespace BadBot.Commands;

public abstract class RequestCommandModule: ApplicationCommandModule
{
	public event Action<string> MessageEmitted;
	public event Action<int> ProgressChanged;
	public void Emit(string message, string end = "\n") => MessageEmitted(message + end);
	public void SetProgress(int percentage) => ProgressChanged(percentage);

	public Request Request { get; private set; }
	private RequestOptions _options;
	public abstract Task Process();
	public abstract Task Execute(InteractionContext ctx, string url = "");
	public abstract bool Video { get; protected set; }
	public DiscordMessage RequestMessage { get; private set; }

	public async Task AddToQueue(RequestOptions options, InteractionContext ctx, string url = "")
	{
		await ctx.CreateResponseAsync("секунду..");
		RequestMessage = await ctx.GetOriginalResponseAsync();
		Request = new Request
		{
			Id = Guid.NewGuid().ToString("N"),
			Type = Request.RequestTypes[GetType()],
			Channel = ctx.Channel,
			User = ctx.User
		};

		var urlSource = string.IsNullOrEmpty(url) ? UrlSource.Attachment : UrlSource.CommandArgument;
		var platformSource = PlatformSource.Invalid;
		if (!string.IsNullOrEmpty(url) && UrlHelper.IsYoutube(url))
			platformSource = PlatformSource.Youtube;
		string mimeType;

		if (string.IsNullOrEmpty(url))
		{
			var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
				.WithContent("пытаюсь получить предыдущие сообщения.."));
			var messages = await ctx.Channel.GetMessagesBeforeAsync(message.Id, 10);

			var urlRegex =
				"https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,4}\\b([-a-zA-Z0-9@:%_\\+.~#?&//=]*)";

			if (messages.All(x => x.Attachments.Count == 0 && !Regex.IsMatch(x.Content, urlRegex)))
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.WithContent("не могу я из прошлих сообщений получить видео/картинку! куда дели??"));
				return;
			}

			var sourceMessage = messages.First(x => x.Attachments.Count != 0 || Regex.IsMatch(x.Content, urlRegex));
			if (sourceMessage.Attachments.Count != 0)
			{
				url = sourceMessage.Attachments[0].Url;
				mimeType = sourceMessage.Attachments[0].MediaType;
			}
			else
			{
				url = Regex.Match(sourceMessage.Content, urlRegex).Value;
				mimeType = UrlHelper.GetMimeType(url);
			}
		}
		else
			mimeType = UrlHelper.GetMimeType(url);

		switch (Video)
		{
			case true when mimeType.StartsWith("image"):
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.WithContent("а вы что мне подсунули? команда видео просит, другое мне не надо.."));
				return;
			case false when mimeType.StartsWith("video"):
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.WithContent("а вы что мне подсунули? команда картинки просит, другое мне не надо.."));
				return;
		}
		
		Request.Source = new Source
		{
			Url = url!,
			UrlSource = urlSource,
			PlatformSource = platformSource,
			MimeType = mimeType
		};
		
		Log.Debug("Creating status message for request with ID {Id}", Request.Id);
		if (ctx.Channel.IsPrivate)
		{
			Request.StatusMessage = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
				.WithContent("здесь будет статус.."));
		}
		else
		{
			var statusChannel = ctx.Channel.Guild.GetChannel(973684193257214032);
			Request.StatusMessage = await statusChannel.SendMessageAsync($"здесь будет статус.. ({Request.Id})");
		}
		
		RequestManager.Singleton.AddRequest(this);
		Log.Debug("Added new request to the queue (ID {Id})", Request.Id);
		
		await ctx.EditResponseAsync(new DiscordWebhookBuilder()
			.AddEmbed(new DiscordEmbedBuilder()
				.WithTitle("запрос создан!")
				.WithColor(DiscordColor.Gold)
				.AddField("ID", $"`{Request.Id}`", true)
				.AddField("Тип", $"`{Request.Type}` ({Request.RequestTypes.First(x => x.Value == Request.Type).Key.Name})", true)
				.AddField("Статус", Request.StatusMessage.JumpLink.ToString())
				.Build()));
	}
}