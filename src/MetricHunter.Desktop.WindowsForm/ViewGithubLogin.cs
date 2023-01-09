using MetricHunter.Desktop.Presenters;
using MetricHunter.Desktop.Properties;
using MetricHunter.Desktop.Views;

namespace MetricHunter.Desktop;

public partial class ViewGithubLogin : Form, IViewGithubLogin
{
    public ViewGithubLogin()
    {
        InitializeComponent();
    }

    public IViewGithubLoginPresenter Presenter { get; set; }

    public void Run()
    {
        ShowDialog();
    }

    private void AuthenticateButton_Click(object sender, EventArgs e)
    {
        Settings.Default.GithubToken = _githubToken.Text;
        Settings.Default.Save();

        Close();
    }
}