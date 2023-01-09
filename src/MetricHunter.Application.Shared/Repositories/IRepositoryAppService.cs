using Octokit;

namespace MetricHunter.Application.Repositories;

public interface IRepositoryAppService
{
    Task<Repository[]> ReadRepositories(string path);

    Task WriteRepositories(IEnumerable<Repository> repositories, string path);
}