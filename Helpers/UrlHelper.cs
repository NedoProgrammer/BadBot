namespace BadBot.Helpers;

public class UrlHelper
{
	public static string GetExtension(string url) => IsYoutube(url) ? ".mp4" : Path.GetExtension(url).Split("?")[0].Trim();
	public static string GetMimeType(string url) => MimeTypes.GetMimeType(GetExtension(url));
	
	public static bool IsYoutube(string url)
	{
		//000000 11111111111 222
		//https:/youtube.com/...
		var domain = url.Replace("//", "/").Split("/")[1];
		return domain is "youtube.com" or "youtu.be" or "www.youtube.com" or "www.youtu.be";
	}
}