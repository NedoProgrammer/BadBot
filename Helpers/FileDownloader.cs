namespace BadBot.Helpers;

public class FileDownloader
{
	public static async Task<byte[]> DownloadByteArray(string url)
	{
		var client = new HttpClient();
		var data = await client.GetByteArrayAsync(url);
		return data;
	}
}