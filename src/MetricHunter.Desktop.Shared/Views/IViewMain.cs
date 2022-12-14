using MetricHunter.Desktop.Core;
using MetricHunter.Desktop.Models;
using MetricHunter.Desktop.Presenters;
using Octokit;

namespace MetricHunter.Desktop.Views;

public interface IViewMain : IView<IViewMainPresenter>
{
    void ShowMessage(string message);
    string GithubToken { get; }
    IEnumerable<Language>? LanguageSelectList { set; }

    IEnumerable<SortDirection> SortDirectionSelectList { set; }

    IEnumerable<long> SelectedRepositories { get; }

    Language? SelectedLanguage { get; }

    SortDirection SortDirection { get; }

    int RepositoryCount { get; }

    string Topics { get; }

    string JsonLoadPath { get; set; }
    
    string JsonSavePath { get; set; }
    
    string DownloadRepositoryPath { get; set; }

    void ShowRepositories(IEnumerable<RepositoryModel> repositories);
}
