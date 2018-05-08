// scanner.ProjectInstaller
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

[RunInstaller(true)]
public class ProjectInstaller : Installer
{
    private IContainer components;
    private ServiceProcessInstaller serviceProcessInstaller1;
    private ServiceInstaller ScannerInstaller;

    public ProjectInstaller() {
        InitializeComponent();
    }

    protected override void Dispose(bool disposing) {
        if (disposing && components != null) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent() {
        serviceProcessInstaller1 = new ServiceProcessInstaller();
        ScannerInstaller = new ServiceInstaller();
        serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
        serviceProcessInstaller1.Password = null;
        serviceProcessInstaller1.Username = null;
        ScannerInstaller.Description = "Служба опроса сканеров";
        ScannerInstaller.DisplayName = "ScannerService";
        ScannerInstaller.ServiceName = "Scanner";
        ScannerInstaller.StartType = ServiceStartMode.Automatic;
        base.Installers.AddRange(new Installer[2]
        {
            serviceProcessInstaller1,
            ScannerInstaller
        });
    }
}