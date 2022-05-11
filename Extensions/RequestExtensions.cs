using BadBot.Helpers;
using BadBot.Requests;

namespace BadBot.Extensions;

public static class RequestExtensions
{
	public static string SourceFullPath(this Request request) =>
		$"{SourceDirectory(request)}\\source{Extension(request)}";
	
	public static string SourceDirectory(this Request request) => 
		$"{Environment.CurrentDirectory}\\Requests\\{request.Id}";

	public static string Extension(this Request request) => UrlHelper.GetExtension(request.Source.Url);
}