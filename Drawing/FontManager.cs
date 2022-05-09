using Serilog;
using SixLabors.Fonts;

namespace BadBot.Drawing;

public class FontManager
{
	private static FontManager? _manager;

	public static FontManager Singleton
	{
		get
		{
			if (_manager == null)
				throw new Exception("Cannot get font manager before Init() was called!");
			return _manager;
		}
	}
	
	private FontManager() {}
	private FontCollection _collection = new();
	public Dictionary<string, FontFamily> Families = new();

	public static void Init()
	{
		_manager = new FontManager();
		_manager.Add("Resources/MainFont.ttf");
		Log.Information("Initialized font manager");
	}

	public void Add(string path)
	{
		Families[Path.GetFileNameWithoutExtension(path)] = _collection.Add(path);
		Log.Debug("Loaded font from {FontPath}", path);
	}
}