using System.Reflection;
using BadBot.Core;
using BadBot.Requests;
using DSharpPlus.SlashCommands;
using BadBot.Extensions;
using ImageMagick;
using Serilog;
using Serilog.Parsing;

namespace BadBot.Commands.Modify.Video;

public class ContentAwareScale: RequestCommandModule
{
	public override async Task Process()
	{
		this.RequireDirectories("Chunks", "ConvertedChunks", "In", "Out");

		if (Request.Extension() != ".mp4")
		{
			Emit("Converting video to mp4 (compatibility reasons)");
			var source = $"source{Request.Extension()}";
			await this.Convert(source, "source.mp4");
			Request.Source.Extension = ".mp4";
		}
		
		await this.ExtractAudio("-filter_complex \"vibrato=f=10:d=0.6\"");
		await this.SplitIntoChunks(30);
		
		foreach (var file in Directory.GetFiles(Request.SourceDirectory() + "\\Chunks"))
			await this.SplitFrames(Path.GetFullPath(file), $"In\\{Path.GetFileNameWithoutExtension(file)}");
		
		var inDirectory = $"{Request.SourceDirectory()}\\In\\";
		var chunkCount = Directory.GetDirectories(inDirectory).Length;
		
		var counter = 0;
		var delta = 80d / chunkCount;
		
		Emit($"Creating image workers ({chunkCount})");
		var list = new List<Task>();

		for (var i = 0; i < chunkCount; i++)
		{
			this.RequireDirectories($"Out\\{i}");
			var index = i;
			list.Add(new Task(() =>
			{
				var start = index * delta;
				var end = (index + 1) * delta;
				var files = Directory.GetFiles(inDirectory + index);
				var perImage = end / files.Length;
				var current = start;
				foreach (var file in files.OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x))))
				{
					ProcessImage($"{Request.SourceDirectory()}\\Out\\{index}\\", Path.GetFullPath(file), current, index);
					current += perImage;
				}
			}));
		}

		while (list.Count > 0)
		{
			Emit($"Starting {counter + 1}st/nd/rd group of workers");
			var tasks = list.Take(5);
			foreach (var task in tasks)
				task.Start();
			await Task.WhenAll(tasks);
			list.RemoveRange(0, list.Count >= 5 ? 5: list.Count);
			counter++;
			Emit("Finished");
		}
		
		Emit("Combining images to videos");
		var fps = await this.GetFps();
		for (var i = 0; i < chunkCount; i++)
		{
			var outDirectory = $"{Request.SourceDirectory()}\\Out\\{i}";
			Emit($"{i + 1}/{chunkCount}");
			await this.ConcatImages(outDirectory, $"{Request.SourceDirectory()}\\ConvertedChunks\\{i}{Request.Extension()}", fps);
		}

		Emit("Combining chunks into video");
		var noAudioFile = $"noaudio{Request.Extension()}";
		await this.Concat("ConvertedChunks", noAudioFile);
		await this.AddAudio(noAudioFile, "audio.mp3", $"result{Request.Extension()}");
	}

	[SlashCommand("cas", "жмыхнуть видео")]
	public override async Task Execute(InteractionContext ctx, [Option("url", "ссылка на видео")] string url = "")
	{
		await AddToQueue(new RequestOptions(), ctx, url);
	}

	public override bool Video { get; protected set; } = true;
	
	private void ProcessImage(string outDirectory, string imagePath, double intensity, int chunkIndex)
	{
		var image = new MagickImage(imagePath);
		image.LiquidRescale(new Percentage((int)(100 - intensity)));
		image.Write(outDirectory + Path.GetFileName(imagePath));
		image.Dispose();
	}
}