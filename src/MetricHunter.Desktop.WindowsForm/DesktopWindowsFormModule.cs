using System.Diagnostics;
using MetricHunter.Application;
using MetricHunter.Core.DependencyProcesses;
using MetricHunter.Core.Modules;
using MetricHunter.Desktop.Core;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace MetricHunter.Desktop;

[DependsOn(typeof(DesktopSharedModule),
    typeof(ApplicationModule))]
public class DesktopWindowsFormModule : MetricHunterModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<ProcessDependencyOptions>(o =>
        {
            o.StartupModule = typeof(DesktopWindowsFormModule);
            o.ErrorAction = pd =>
            {
                var text = pd?.ErrorMessage + Environment.NewLine + "Do you want to download it now?";
                if (MessageBox.Show(text, pd?.ErrorTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) ==
                    DialogResult.Yes)
                    if (pd?.DownloadUrl != null)
                        Process.Start("explorer", pd.DownloadUrl);
                Environment.Exit(0);
            };
        });

        base.ConfigureServices(context);
    }

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        Configure<ProcessDependencyOptions>(o => o.StartupModule = typeof(DesktopWindowsFormModule));

        base.PreConfigureServices(context);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.ServiceProvider
            .GetRequiredService<IApplicationController>();

        app.StartApplication();

        base.OnApplicationInitialization(context);
    }
}