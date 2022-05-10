namespace BadBot.Requests;

public enum RequestDataType
{
	Threads,
	Speed
	//TODO
}

public class RequestOptions
{
	public List<RequestDataType> RequiredData = new();
}