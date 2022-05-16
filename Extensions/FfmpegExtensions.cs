using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using BadBot.Commands;
using BadBot.Helpers;
using BadBot.Requests;
using Serilog;

namespace BadBot.Extensions;

public static class FfmpegExtensions
{
	public static async Task ReverseSource(this RequestCommandModule module)
	{
		RequireDirectories(module, "Chunks", "ReversedChunks");
		module.Emit("Reversing source");
		var request = module.Request;
		module.Emit("Splitting source into chunks of 300 seconds");
		
		var chunkDirectory = $"{request.SourceDirectory()}\\Chunks";
		var reversedChunkDirectory = $"{request.SourceDirectory()}\\ReversedChunks";
		await SplitIntoChunks(module, 300);
		
		module.Emit("Reversing chunks");
		var files = Directory.GetFiles(chunkDirectory).OrderByDescending(x => int.Parse(Path.GetFileNameWithoutExtension(x)));
		foreach (var file in files)
		{
			module.Emit(Path.GetFileName(file));
			await Execute(
				$"-i \"{Path.GetFileName(file)}\" -vf reverse -af areverse \"{reversedChunkDirectory}\\{Path.GetFileNameWithoutExtension(file)}{request.Extension()}\"",
				chunkDirectory);
		}

		await Concat(module, "ReversedChunks", $"result{request.Extension()}");
	}

	public static async Task AddAudio(this RequestCommandModule module, string to, string audio, string output)
	{
		module.Emit("Adding audio to video");
		await Execute($"-i \"{to}\" -i \"{audio}\" -map 0:v -map 1:a -c:v copy \"{output}\"", module.Request.SourceDirectory());
	}

	public static async Task SplitIntoChunks(this RequestCommandModule module, int length)
	{
		RequireDirectories(module, "Chunks");
		module.Emit($"Splitting source into chunks of {length} seconds");
		var request = module.Request;
		await Execute($"-i \"{request.SourceFullPath()}\" -map 0 -c copy -f segment -segment_time {length} -reset_timestamps 1 Chunks\\%0d{request.Extension()}", request.SourceDirectory());
	}

	public static async Task Convert(this RequestCommandModule module, string from, string to)
	{
		module.Emit($"Converting {from} to {to}");
		//:D
		await Execute($"-i \"{from}\" \"{to}\"", module.Request.SourceDirectory());
	}

	public static async Task Concat(this RequestCommandModule module, string from, string to, bool reversed = true)
	{
		RequireDirectories(module, from);
		module.Emit("Concatenating videos");
		var request = module.Request;
		var videos = Directory.GetFiles(Path.Join(request.SourceDirectory(), from))
			.OrderByDescending(x => int.Parse(Path.GetFileNameWithoutExtension(x))).ToList();
		if (!reversed)
			videos.Reverse();
		var str = videos.Aggregate("", (current, file) => current + $"file '{Path.GetFullPath(file)}'");
		await File.WriteAllTextAsync($"{request.SourceDirectory()}\\Files.txt", str);
		await Execute($"-f concat -safe 0 -i Files.txt -c copy {to}", request.SourceDirectory());
	}

	public static async Task SplitFrames(this RequestCommandModule module, string file, string to)
	{
		RequireDirectories(module, to);
		module.Emit($"Splitting video {Path.GetFileName(file)} into frames");
		await Execute($"-i \"{file}\" {to}\\%d.png", module.Request.SourceDirectory());
	}

	public static async Task ExtractAudio(this RequestCommandModule module, string effects = "")
	{
		module.Emit("Extracting audio from video");
		await Execute($"-i \"{module.Request.SourceFullPath()}\" -q:a 0 -map a {effects} {module.Request.SourceDirectory()}\\audio.mp3");
	}

	public static async Task ConcatImages(this RequestCommandModule module, string from, string to, string fps)
	{
		//RequireDirectories(module, to);
		module.Emit($"Combining images from {from} to video");
		await Execute($"-r {fps} -i \"{from}\\%d.png\" -r {fps} -vf format=yuv420p \"{to}\"");
	}
	
	public static async Task<string> GetFps(this RequestCommandModule module)
	{
		var pci = new ProcessStartInfo
		{
			FileName = "ffprobe",
			Arguments = $"-v error -select_streams v:0 -show_entries stream=r_frame_rate -of csv=p=0 \"{module.Request.SourceFullPath()}\"",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true
		};
		var p = new Process {StartInfo = pci};
		var result = "";
		p.OutputDataReceived += (_, args) =>
		{
			if (args.Data == null) return;
			result = args.Data;
		};
		p.Start();
		p.BeginOutputReadLine();
		await p.WaitForExitAsync();
		return result;
	}

	private static async Task Execute(string args, string environment = "")
	{
		var pci = new ProcessStartInfo
		{
			FileName = "ffmpeg",
			Arguments = args,
			WorkingDirectory = string.IsNullOrEmpty(environment) ? Environment.CurrentDirectory : environment,
			UseShellExecute = false,
			//UseShellExecute = true,
			RedirectStandardError = true,
			//WindowStyle = ProcessWindowStyle.Hidden
		};
		var process = Process.Start(pci)!;
		process.ErrorDataReceived += (_, e) => Log.Information(e.Data);
		process.BeginErrorReadLine();
		await process.WaitForExitAsync();
	}

	public static void RequireDirectories(this RequestCommandModule module, params string[] directories)
	{
		foreach (var directory in directories)
		{
			var required = Path.Join(module.Request.SourceDirectory(), directory);
			if (!Directory.Exists(required))
				Directory.CreateDirectory(required);
		}
	}
}