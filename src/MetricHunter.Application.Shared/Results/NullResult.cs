namespace MetricHunter.Application.Results;

public class NullResult : IResult
{
    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>();
    }

    public List<Dictionary<string, string>> ToDictionaryListByTopics()
    {
        return new List<Dictionary<string, string>>();
    }

    public bool IsEmpty()
    {
        return true;
    }
}