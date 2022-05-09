namespace BadBot.Helpers;

public class LinkHelper
{
	public bool IsYoutube(string url)
	{
		//000000 11111111111 222
		//https:/youtube.com/...
		var domain = url.Replace("//", "/").Split("/")[1];
		return domain is "youtube.com" or "youtu.be";
	}
}