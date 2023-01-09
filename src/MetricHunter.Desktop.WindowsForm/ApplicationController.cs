using MetricHunter.Desktop.Core;
using MetricHunter.Desktop.Presenters;
using MetricHunter.Desktop.Views;
using Volo.Abp.DependencyInjection;

namespace MetricHunter.Desktop;

public class ApplicationController : IApplicationController, ISingletonDependency
{
    public ApplicationController(IServiceProvider serviceProvider, IViewMain viewMain)
    {
        ServiceProvider = serviceProvider;
        ViewMain = viewMain;
    }

    public IViewMain ViewMain { get; }

    public void ErrorMessage(string message)
    {
        MessageBox.Show(message);
    }

    public void SuccessMessage(string message)
    {
        throw new NotImplementedException();
    }

    public IServiceProvider ServiceProvider { get; }

    public void StartApplication()
    {
        var presenter = new ViewMainPresenter(ViewMain, this);
        presenter.Run();
    }

    public void ShowGithubLogin()
    {
        using var viewGithubLogin = new ViewGithubLogin();
        var presenter = new ViewGithubLoginPresenter(this, viewGithubLogin);
        presenter.Run();
    }

    public void ShowMessage(string message)
    {
        ViewMain.ShowMessage(message);
    }
}