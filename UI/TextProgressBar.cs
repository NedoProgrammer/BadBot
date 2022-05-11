namespace BadBot.UI;

public class TextProgressBar
{
	public int Max { get; }
	public int Min { get; }
	public int Length { get; }
	public int Value;
	private int _delta;

	public TextProgressBar(int max = 100, int min = 0, int length = 20)
	{
		Max = max;
		Min = min;
		Length = length;
		_delta = Max / Length;
	}

	public override string ToString()
	{
		var str = "[";
		var percentage = (int)((Value + Min) / (float)Max * 100);
		for (var i = 0; i < percentage / _delta; i++)
			str += "=";
		for (var i = percentage / _delta; i < Length; i++)
			str += " ";
		str += $"] {percentage}%";
		return str;
	}
}