using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace GTASolo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StatusLabel.Visibility = Visibility.Hidden;
            if (FindGTA() == true)
            {
                StatusLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                StatusLabel.Content = "GTA Not Running!";
                StatusLabel.Visibility = Visibility.Visible;
            }
      
        }

        private bool FindGTA()
        {
            Process[] gta = Process.GetProcessesByName("gta5");
            if (gta.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);
        private static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid); // throws exception if process does not exist

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }
        private void PauseGTA()
        {
            ActivateButton.IsEnabled = false;
            Process GTAInst = Process.GetProcessesByName("gta5")[0];
            SuspendProcess(GTAInst.Id);
            Thread.Sleep(8000);
            StatusLabel.Content = "GTA unpaused! enjoy!";
            ResumeProcess(GTAInst.Id);
            Thread.Sleep(200);
            ActivateButton.IsEnabled = true;
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            if(FindGTA() == true)
            {
                StatusLabel.Visibility = Visibility.Visible;
                StatusLabel.Content = "suspending GTA.. please wait 8 seconds";
                PauseGTA();
            }
            else
            {
                StatusLabel.Content = "GTA Not Running!";
                StatusLabel.Visibility = Visibility.Visible;
            }
        }
    }
}
