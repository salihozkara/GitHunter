﻿using Octokit;

namespace GitHunter.Application.Git;

public class GitOutput
{
    public GitOutput(IReadOnlyList<Repository> resultItems, HashSet<SearchRepositoriesRequest> failedRequests)
    {
        Repositories = resultItems;
        FailedRequests = failedRequests;
    }

    public IReadOnlyList<Repository> Repositories { get; }
    public HashSet<SearchRepositoriesRequest> FailedRequests { get; }
}