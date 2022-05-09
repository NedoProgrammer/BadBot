using System.Numerics;
using BadBot.Core;
using BadBot.Helpers;
using DSharpPlus;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace BadBot.Drawing;

public class CrashedDrawer
{
	public static async Task<MemoryStream> Draw(DiscordClient client, Exception e)
	{
		var ownerUrl = (await client.GetUserAsync(Config.OwnerId)).GetAvatarUrl(ImageFormat.Png);
		var botUrl = client.CurrentUser.GetAvatarUrl(ImageFormat.Png);

		var ownerImage = Image.Load(await FileDownloader.DownloadByteArray(ownerUrl));
		var botImage = Image.Load(await FileDownloader.DownloadByteArray(botUrl));
		var mainImage = await Image.LoadAsync("Resources/Crashed.jpg");

		var font = new Font(FontManager.Singleton.Families["MainFont"], 24);
		
		ownerImage.Mutate(x => x.Rotate(-45).Resize(180,180));
		botImage.Mutate(x => x.Rotate(-90).Resize(180, 180));
		mainImage.Mutate(x =>
		{
			x.DrawImage(ownerImage, new Point(60, 0), 1f);
			x.DrawImage(botImage, new Point(-10, 225), 1f);
			x.DrawText(new DrawingOptions(), new TextOptions(font)
				{
					WordBreaking = WordBreaking.Normal,
					WrappingLength = mainImage.Width,
					TextDirection = TextDirection.LeftToRight,
					Font = font,
					Origin = new Vector2(0, 0)
				},e + "\n" + e.StackTrace, new SolidBrush(Color.White), new Pen(Color.Black, 1f));
		});

		var stream = new MemoryStream();
		await mainImage.SaveAsPngAsync(stream);
		return stream;
	}
}