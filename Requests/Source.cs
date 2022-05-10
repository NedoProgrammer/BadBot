namespace BadBot.Requests;

public enum UrlSource
{
	Attachment,
	CommandArgument
}

public enum PlatformSource
{
	Youtube,
	Invalid
}

public class Source
{
	public string MimeType;
	public string Url;
	public UrlSource UrlSource;
	public PlatformSource PlatformSource = PlatformSource.Invalid;
}