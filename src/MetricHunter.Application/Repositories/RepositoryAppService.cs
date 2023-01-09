using System.Text;
using MetricHunter.Application.Resources;
using Newtonsoft.Json;
using Octokit;
using Volo.Abp.DependencyInjection;
using FileMode = System.IO.FileMode;

namespace MetricHunter.Application.Repositories;

public class RepositoryAppService : IRepositoryAppService, ISingletonDependency
{
    public async Task<Repository[]> ReadRepositories(string path)
    {
        return JsonConvert.DeserializeObject<Repository[]>(await File.ReadAllTextAsync(path),
            Resource.Jsons.JsonSerializerSettings) ?? Array.Empty<Repository>();
    }

    public async Task WriteRepositories(IEnumerable<Repository> repositories, string path)
    {
        var addedRepositories = new List<Repository>();

        if (File.Exists(path))
        {
            var availableRepositories = await ReadRepositories(path);

            if (availableRepositories.Any()) addedRepositories.AddRange(availableRepositories);
        }

        addedRepositories.AddRange(repositories);

        var json = JsonConvert.SerializeObject(addedRepositories, Resource.Jsons.JsonSerializerSettings);

        // Save file with create mode
        await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,
            FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await fileStream.WriteAsync(Encoding.UTF8.GetBytes(json));
    }
}