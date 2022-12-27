﻿using GitHunter.Desktop.Core;
using GitHunter.Desktop.Views;

namespace GitHunter.Desktop.Presenters;

public interface IViewMainPresenter : IPresenter<IViewMain>
{
    void LoadForm();
    void ShowGithubLogin();
    Task SearchRepositories();
    Task<string> CalculateMetrics();
    Task DownloadRepositories();
    void ShowRepositories();
    Task SaveRepositories();
}