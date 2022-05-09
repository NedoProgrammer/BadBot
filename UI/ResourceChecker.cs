using System.Diagnostics;
using Spectre.Console;

namespace BadBot.UI;

/// <summary>
/// A friendly UI for displaying the process of
/// checking resources.
/// </summary>
public class ResourceChecker
{
	/// <summary>
	/// Start the process.
	/// </summary>
	public static void Show()
	{
		AnsiConsole.Clear();
		AnsiConsole.MarkupLine("[yellow]Checking resources..[/]");
		
		CheckResource("config.json", false, false, str => File.WriteAllText(str, "{}"));
		CheckResource("Resources", true, false, str => Directory.CreateDirectory(str));
		CheckResource("Resources/Crashed.jpg", false, true);
		CheckResource("Resources/MainFont.ttf", false, true);
		
		AnsiConsole.MarkupLine("\n[yellow]Checking commands..[/]");
		CheckCommand("ffmpeg", "", 1, 0);
		CheckCommand("python", "--version");
		CheckCommand("node", "--version");
	}

	/// <summary>
	/// Search a file or directory.
	/// If fatal is true, will exit the app.
	/// If fix is specified, and the resource was not found, it will call that function.
	/// </summary>
	/// <param name="name">Name of the resource.</param>
	/// <param name="directory">Is it a directory?</param>
	/// <param name="fatal">If it does not exist, is it fatal?</param>
	/// <param name="fix">The fix for a missing resource (if possible)</param>
	private static void CheckResource(string name, bool directory = false, bool fatal = false, Action<string>? fix = null)
	{
		AnsiConsole.Markup($"[white]Searching for \"{name}\" ({(directory ? "directory" : "file")}).. [/]");
		
		if ((!File.Exists(name) && !directory && fatal) || (!Directory.Exists(name) && directory && fatal))
		{
			AnsiConsole.MarkupLine("[bold red]NOT FOUND[/]");
			AnsiConsole.MarkupLine($"[bold red]Resource \"{name}\" was not found! The bot will now exit.[/]");
			Environment.Exit(-1);
		}

		if ((!File.Exists(name) && !directory) || (!Directory.Exists(name) && directory))
		{
			fix?.Invoke(name);
			AnsiConsole.MarkupLine("[yellow]FIXED[/]");
		}
		else AnsiConsole.MarkupLine("[green]OK[/]");
	}

	private static void CheckCommand(string filename, string arguments = "", int successCode = 0, int failCode = 1)
	{
		var formatted = $"\"{filename}{(string.IsNullOrEmpty(arguments) ? "": " ")}{arguments}\"";
		AnsiConsole.Markup($"[white]Checking command {formatted}.. [/]");
		var pci = new ProcessStartInfo
		{
			FileName = filename,
			Arguments = arguments,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		try
		{
			var process = Process.Start(pci)!;
			process.WaitForExit();
			var exitCode = process.ExitCode;
			if(exitCode == successCode)
				AnsiConsole.MarkupLine("[green]OK[/]");
			else
			{
				AnsiConsole.MarkupLine(exitCode == failCode ? $"[red]FAILED\nCommand {formatted} exited with a failure code ({failCode})! The bot will not exit.[/]":
					$"[yellow]UNKNOWN[/]\nCommand {formatted} exited with code {exitCode}, which was not intended. The bot will now exit.");
				Environment.Exit(-1);
			}

		}
		catch (Exception e)
		{
			AnsiConsole.MarkupLine($"[red]FAILED\nUnexpected error occured while trying to run {formatted} ({e.Message}). The bot will now exit.[/]");
			Environment.Exit(-1);
		}
	}
}