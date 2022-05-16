using System.Diagnostics;
using BadBot.Requests;

namespace BadBot.Helpers;

public class FileDownloader
{
	public static async Task<byte[]> DownloadByteArray(string url)
	{
		var client = new HttpClient();
		var data = await client.GetByteArrayAsync(url);
		return data;
	}

	public static async Task DownloadSource(Request request)
	{
		var requestDirectory = $"{Environment.CurrentDirectory}\\Requests\\{request.Id}";
		if (!Directory.Exists(requestDirectory))
			throw new Exception("Request directory does not exist!");
		
		var bytes = await DownloadByteArray(request.Source.Url);

		var extension = UrlHelper.GetExtension(request.Source.Url);
		var file = $"{requestDirectory}\\source{extension}";
		await File.WriteAllBytesAsync(file, bytes);
	}

	public static async Task DownloadYoutube(Request request)
	{
		var requestDirectory = $"{Environment.CurrentDirectory}\\Requests\\{request.Id}";
		if (!Directory.Exists(requestDirectory))
			throw new Exception("Request directory does not exist!");

		var pci = new ProcessStartInfo
		{
			FileName = "yt-dlp",
			Arguments = $"{request.Source.Url} -f \"(mp4)[height<=720]\" -o \"{requestDirectory}\\source.mp4\"",
			UseShellExecute = true,
			WindowStyle = ProcessWindowStyle.Hidden
		};
		var process = Process.Start(pci);
		await process!.WaitForExitAsync();
	}
}