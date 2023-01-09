using MetricHunter.Desktop.Views;

namespace MetricHunter.Desktop.Core;

public interface IApplicationController
{
    IViewMain ViewMain { get; }

    public IServiceProvider ServiceProvider { get; }

    void ErrorMessage(string message);

    void SuccessMessage(string message);

    void ShowGithubLogin();

    void StartApplication();
}