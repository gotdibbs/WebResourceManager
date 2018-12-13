using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WebResourceManager.Models;

namespace WebResourceManager.Helpers
{
    public static class FiddlerHelper
    {
        public static void ExecAction(Guid profileId)
        {
            var installLocation = GetFiddlerLocation();

            if (string.IsNullOrEmpty(installLocation) || !Directory.Exists(installLocation))
            {
                Alert.Show("Failed to locate path for Fiddler. Please check Fiddler is installed");
                return;
            }

            var pname = Process.GetProcessesByName("fiddler");
            
            if (pname == null || pname.Length == 0)
            {
                Process.Start(Path.Combine(installLocation, "Fiddler.exe"));
            }

            // Try 10 times, waiting for fiddler to open
            for (var i = 0; i < 10; i++)
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = Path.Combine(installLocation, "ExecAction.exe"),
                        Arguments = $"imposter.{profileId}"
                    };
                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode == 2)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            Alert.Show("Failed to execute the intended action in Fiddler.");
        }

        public static string GetFiddlerLocation()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Fiddler2\\InstallerSettings"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("InstallPath");
                        if (o != null)
                        {
                            var path = o as string;
                            path = path.Substring(0, path.Length - 1);
                            return path;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // do something with this
            }

            return null;
        }
    }
}
