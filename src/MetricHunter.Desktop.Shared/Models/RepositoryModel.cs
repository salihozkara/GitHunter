namespace MetricHunter.Desktop.Models;

public class RepositoryModel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Owner { get; set; }
    public string Description { get; set; }
    public int Stars { get; set; }
    public string Url { get; set; }
    public string License { get; set; }
    public string SizeString { get; set; }
}