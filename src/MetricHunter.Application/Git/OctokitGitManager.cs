using Microsoft.Extensions.Logging;
using Octokit;
using Volo.Abp.DependencyInjection;
using Range = Octokit.Range;

namespace MetricHunter.Application.Git;

public class OctokitGitManager : IGitManager, ITransientDependency
{
    private const int MaxPage = 10;
    private const int PerPage = 100;
    private static readonly GitHubClient Client = new(new ProductHeaderValue("MetricHunter"));
    private readonly ILogger<OctokitGitManager> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OctokitGitManager" /> class.
    /// </summary>
    /// <param name="logger"></param>
    public OctokitGitManager(ILogger<OctokitGitManager> logger)
    {
        _logger = logger;
    }


    /// <summary>
    ///     This event is triggered when a request fails.
    /// </summary>
    public event EventHandler<RequestErrorEventArgs>? SearchRepositoriesRequestError;

    /// <summary>
    ///     This event is triggered when a request succeeds.
    /// </summary>
    public event EventHandler<RequestSuccessEventArgs>? SearchRepositoriesRequestSuccess;

    /// <summary>
    ///     This event is triggered when rate limit is reached.
    /// </summary>
    public event EventHandler<RateLimitExceededEventArgs>? RateLimitExceeded;

    /// <summary>
    ///     This event is triggered when there is an unknown error in a request.
    /// </summary>
    public event EventHandler<ExceptionEventArgs>? OnException;

    /// <summary>
    ///     User login
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public void Authenticate(string username, string password)
    {
        Client.Credentials = new Credentials(username, password);
    }

    /// <summary>
    ///     User login
    /// </summary>
    /// <param name="token"></param>
    public void Authenticate(string token)
    {
        Client.Credentials = new Credentials(token);
    }

    /// <summary>
    ///     Lists repository information from github by input
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<GitOutput> GetRepositoriesAsync(GitInput input)
    {
        List<SearchRepositoriesRequest> failedRequests = new();
        List<Repository> repositories = new();
        Range? stars = null;

        while (repositories.Count < input.Count)
        {
            var requests = GetPageNumbers(input.Count - repositories.Count).Select(p => CreateRequest(input, p, stars))
                .ToList();
            var results = await RunRequests(requests, failedRequests);
            var items = ConvertToRepositories(results);
            repositories.AddRange(items);
            var count = repositories.Count;
            repositories = repositories.DistinctBy(r => r.CloneUrl).ToList();
            var newCount = repositories.Count;
            if (count != newCount)
                _logger.LogWarning("{count} repositories were removed because of duplication", count - newCount);

            if (results.Where(r => r != null).Any(i => i!.Items.Count != PerPage))
            {
                _logger.LogWarning("No more repositories found");
                break;
            }

            // Updating range as only the first 1000 search results are available in Github requests
            if (repositories.Any())
                stars = input.Order == SortDirection.Descending
                    ? Range.LessThanOrEquals(repositories.Min(x => x.StargazersCount))
                    : Range.GreaterThanOrEquals(repositories.Max(x => x.StargazersCount));
        }

        return new GitOutput(repositories.Take(input.Count).ToList(), failedRequests);
    }

    public async Task<bool> IsRepositoryPublicAsync(Repository repository)
    {
        var defaultDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        if (repository.CreatedAt < defaultDate || repository.UpdatedAt < defaultDate)
            return false;
        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync(repository.HtmlUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to check if repository is public");
            return false;
        }
    }

    /// <summary>
    ///     Reruns failed requests
    /// </summary>
    /// <param name="failedRequests"></param>
    /// <returns></returns>
    public async Task<GitOutput> RetryFailedRequestAsync(List<SearchRepositoriesRequest> failedRequests)
    {
        _logger.LogWarning("Retrying failed requests");
        var requests = failedRequests.ToList();
        failedRequests.Clear();
        var results = await RunRequests(requests, failedRequests);
        _logger.LogWarning("Retrying finished");
        return new GitOutput(ConvertToRepositories(results), failedRequests);
    }

    /// <summary>
    ///     Converts search results to repositories
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    private IReadOnlyList<Repository> ConvertToRepositories(params SearchRepositoryResult?[] results)
    {
        return results.Where(r => r is { Items.Count: > 0 }).SelectMany(r => r!.Items).ToList();
    }

    /// <summary>
    ///     Generates page numbers based on the requested repository number
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private IEnumerable<int> GetPageNumbers(int count)
    {
        if (count <= 0) return ArraySegment<int>.Empty;

        var pages = Math.DivRem(count, PerPage, out var rem);
        if (rem != 0) pages++;

        return Enumerable.Range(0, pages).Select(i => i % MaxPage + 1);
    }

    /// <summary>
    ///     Runs requests
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="failedRequests"></param>
    /// <returns></returns>
    private async Task<SearchRepositoryResult?[]> RunRequests(IEnumerable<SearchRepositoriesRequest> requests,
        List<SearchRepositoriesRequest> failedRequests)
    {
        var tasks = requests.Select(r => RunRequest(r, failedRequests));
        return await Task.WhenAll(tasks);
    }

    /// <summary>
    ///     Runs a single request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="failedRequests"></param>
    /// <returns></returns>
    private async Task<SearchRepositoryResult?> RunRequest(SearchRepositoriesRequest request,
        List<SearchRepositoriesRequest> failedRequests)
    {
        if (!failedRequests.Contains(request))
            failedRequests.Add(request);
        SearchRepositoryResult? result = null;
        try
        {
            result = await Client.Search.SearchRepo(request);
        }
        catch (RateLimitExceededException)
        {
            await RateLimitWait();
            try
            {
                _logger.LogWarning("Retrying request");
                result = await RunRequest(request, failedRequests);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Retrying failed");
                SearchRepositoriesRequestError?.Invoke(this, new RequestErrorEventArgs(request, exception));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Request failed");
            SearchRepositoriesRequestError?.Invoke(this, new RequestErrorEventArgs(request, e));
        }

        if (result is not null)
        {
            failedRequests.Remove(request);
            SearchRepositoriesRequestSuccess?.Invoke(this, new RequestSuccessEventArgs(request, result));
        }

        return result;
    }

    /// <summary>
    ///     Waits for rate limit to reset or client handle to be available
    /// </summary>
    private async Task RateLimitWait()
    {
        try
        {
            var rateLimits = await Client.RateLimit.GetRateLimits();
            // wait rate limit
            if (rateLimits.Resources.Search.Remaining == 0)
            {
                var args = new RateLimitExceededEventArgs(rateLimits.Resources.Search.Reset);
                RateLimitExceeded?.Invoke(this, args);
                if (args.Wait)
                {
                    var resetTime = rateLimits.Resources.Search.Reset;
                    var waitTime = resetTime - DateTimeOffset.Now;
                    if (waitTime.TotalSeconds > 0)
                    {
                        _logger.LogWarning("Rate limit exceeded, waiting {0} seconds", waitTime.TotalSeconds);
                        await Task.Delay(waitTime);
                        _logger.LogWarning("Rate limit reset");
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Rate limit check failed");
            // wait 1 minute
            var args = new ExceptionEventArgs(e);
            OnException?.Invoke(this, args);
            if (args.ThrowException)
                throw;
            if (args.Retry)
                await RateLimitWait();
            if (args.Handled)
                return;
            if (args.DefaultExceptionHandling)
            {
                _logger.LogWarning("Waiting 1 minute");
                await Task.Delay(TimeSpan.FromMinutes(1));
                _logger.LogWarning("Wait finished");
            }
        }
    }

    private SearchRepositoriesRequest CreateRequest(GitInput input, int page, Range? stars)
    {
        var request = new SearchRepositoriesRequest
        {
            Language = input.Language,
            Topic = input.Topic.IsNullOrWhiteSpace() ? null : input.Topic,
            SortField = RepoSearchSort.Stars,
            Stars = stars,
            Page = page,
            PerPage = PerPage,
            Order = input.Order
        };
        return request;
    }
}