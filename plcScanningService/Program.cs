using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

internal static class Program
{
    public static int Main(string[] args) {
        //#if DEBUG
        //Service1 myService1 = new Service1();
        //myService1.OnDebug();
        //System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        //#else

        //endif
        if (Environment.UserInteractive) {
            string a = args[0].ToLowerInvariant().Substring(0, 2);
            if (!(a == "/i")) {
                if (a == "/u") {
                    return UninstallService();
                }
                Console.WriteLine("Argument not recognized: {0}", args[0]);
                Console.WriteLine(string.Empty);
                return 1;
            }
            return InstallService();
        }
        ServiceBase.Run(new ServiceBase[1]
        {
            new Service1()
        });
        return 1;
    }

    private static int InstallService() {
        new Service1();
        try {
            Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\Scanner", "config", "C:\\temp\\config.xml", RegistryValueKind.ExpandString);
            ManagedInstallerClass.InstallHelper(new string[1]
            {
                Assembly.GetExecutingAssembly().Location
            });
        }
        catch (Exception ex) {
            if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Win32Exception)) {
                Win32Exception ex2 = (Win32Exception)ex.InnerException;
                Console.WriteLine("Error(0x{0:X}): Service already installed!", ex2.ErrorCode);
                return ex2.ErrorCode;
            }
            Console.WriteLine(ex.ToString());
            return -1;
        }
        return 0;
    }

    private static int UninstallService() {
        new Service1();
        try {
            ManagedInstallerClass.InstallHelper(new string[2]
            {
                "/u",
                Assembly.GetExecutingAssembly().Location
            });
        }
        catch (Exception ex) {
            if (ex.InnerException.GetType() == typeof(Win32Exception)) {
                Win32Exception ex2 = (Win32Exception)ex.InnerException;
                Console.WriteLine("Error(0x{0:X}): Service not installed!", ex2.ErrorCode);
                return ex2.ErrorCode;
            }
            Console.WriteLine(ex.ToString());
            return -1;
        }
        return 0;
    }
}