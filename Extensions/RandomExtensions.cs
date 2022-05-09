namespace BadBot.Extensions;

public static class RandomExtensions
{
	public static Random New() => new(Guid.NewGuid().GetHashCode());

	public static T NextElement<T>(this Random random, IEnumerable<T> list)
	{
		var enumerable = list as T[] ?? list.ToArray();
		return enumerable.ToArray()[random.Next(0, enumerable.Length)];
	}
}