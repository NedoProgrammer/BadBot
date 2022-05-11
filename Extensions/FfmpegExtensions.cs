using System.Diagnostics;
using BadBot.Commands;
using BadBot.Helpers;
using BadBot.Requests;
using Serilog;

namespace BadBot.Extensions;

public static class FfmpegExtensions
{
	public static async Task ReverseSource(this RequestCommandModule module)
	{
		module.Emit("Reversing source");
		var request = module.Request;
		module.Emit("Splitting source into chunks of 300 seconds");

		var chunkDirectory = $"{request.SourceDirectory()}\\Chunks";
		var reversedChunkDirectory = $"{request.SourceDirectory()}\\ReversedChunks";
		Directory.CreateDirectory(chunkDirectory);
		Directory.CreateDirectory(reversedChunkDirectory);
		await Execute($"-i \"{request.SourceFullPath()}\" -map 0 -c copy -f segment -segment_time 300 -reset_timestamps 1 Chunks\\%03d{request.Extension()}", request.SourceDirectory());
		
		module.Emit("Reversing chunks");
		var files = Directory.GetFiles(chunkDirectory).OrderByDescending(x => int.Parse(Path.GetFileNameWithoutExtension(x)));
		foreach (var file in files)
		{
			module.Emit(file);
			await Execute(
				$"-i {Path.GetFileName(file)} -vf reverse \"{reversedChunkDirectory}\\{Path.GetFileNameWithoutExtension(file)}_reversed{request.Extension()}\"");
		}
	}

	private static async Task Execute(string args, string environment = "")
	{
		var pci = new ProcessStartInfo
		{
			FileName = "ffmpeg",
			Arguments = args,
			WorkingDirectory = string.IsNullOrEmpty(environment) ? Environment.CurrentDirectory : environment,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		var process = Process.Start(pci)!;
		process.OutputDataReceived += (_, e) => Log.Information(e.Data);
		process.BeginOutputReadLine();
		await process.WaitForExitAsync();
	}
}