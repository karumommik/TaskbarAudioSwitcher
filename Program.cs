using System;
using System.Windows.Forms;
using TaskbarAudioSwitcher.Native;
using TaskbarAudioSwitcher.UI;

namespace TaskbarAudioSwitcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try 
            {
                // In modern .NET, HighDPI mode can also be set explicitly, 
                // though it is configured via .csproj. We call SetProcessDPIAware as fallback.
                Win32.SetProcessDPIAware(); 
            } 
            catch { }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new AudioWidgetForm());
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.File.WriteAllText(
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crashlog.txt"),
                        ex.ToString()
                    );
                }
                catch { }
                MessageBox.Show(ex.ToString(), "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
