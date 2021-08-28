using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Management;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using Application = SessionSaver.Model.Application;

namespace SessionSaver.DataAccess
{
    public static class ApplicationDA
    {
        private static readonly string currentUserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        private static Dictionary<string, string> commandLines;

        public static List<Application> GetUserApplications()
        {
            IEnumerable<Process> processes = GetCurrentSessionProcessesWithActiveWindows();
            commandLines = GetProcessesCommandLines();
            List<Application> list = new List<Application>();
            
            foreach (Process process in processes)
            {
                Application application = new Application();

                try
                {
                    application = GetApplicationPropertiesFromProcess(process);
                    if (IsAppDataComplete(application))
                    {
                        list.Add(application);
                    }
                }
                catch (Exception)
                {
                    // Some processes are not accessible because access is denied or they exited, 
                    // for these cases exceptions are thrown when trying to access their properties.
                    // These processes are removed in GetCurrentSessionProcessesWithActiveWindows(), but just in case I add the try-catch block.
                    continue;
                }
            }
            
            return list.OrderBy(x => x.DescriptiveName).ThenBy(x => x.Title).ToList();
        }

        private static Application GetApplicationPropertiesFromProcess(Process process)
        {
            Application application = new Application();
            application.HasWindow = true;
            application.Id = process.Id;
            application.Name = process.ProcessName;
            application.Title = RemoveSpecialCharacters(process.MainWindowTitle);
            application.DescriptiveName = GetProcessDescriptiveName(process);
            application.Owner = GetProcessOwner(process);
            application.CommandLine = BuildProcessCommandLine(process.Id);
            application.FileName = process.MainModule.FileName;
            application.StartTime = process.StartTime.ToString();
            application.Icon = GetIconSourceFromFileName(application.FileName);
            return application;
        }

        // Builds the command line that will be used to reopen the application 
        private static string BuildProcessCommandLine(int processId)
        {
            var applicationCommand = commandLines[processId.ToString()];

            if (string.IsNullOrEmpty(applicationCommand))
                return string.Empty;

            applicationCommand = "start " + "\"\"" + " " + applicationCommand;
            return applicationCommand;
        }

        // Extracts icon from process and converts it to be usable in WPF UI
        private static BitmapSource GetIconSourceFromFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            using (Icon ico = Icon.ExtractAssociatedIcon(fileName))
            {
                return Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        // Remove possible linebreaks and similar characters
        private static string RemoveSpecialCharacters(string title)
        {
            return Regex.Replace(title, @"(?:\r\n|[\r\n\u000B\u000C\u0085\u2028\u2029])?", string.Empty);
        }

        // Revisa que los datos necesarios para listar la aplicación estén completos
        private static bool IsAppDataComplete(Application application)
        {
            if (!string.IsNullOrEmpty(application.DescriptiveName) // Cuando estos datos están vacíos, significa que ocurrió una excepción al intentar obtenerlos(acceso denegado o proceso terminado).
                && !string.IsNullOrEmpty(application.CommandLine)
                && !string.IsNullOrEmpty(application.Owner)
                && application.Owner == currentUserName) // El usuario actual es propietario de la aplicación
            {
                return true;
            }
            return false;
        }


        // Creé este procedimiento basándome en otro, para mejor primero obtener toda la lista de commandlines con una sola llamada a ManagementObjectSearcher
        // y no con 'n' llamadas, que es como estaba antes, ya que provocaba lentitud en la carga.
        private static Dictionary<string, string> GetProcessesCommandLines()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessId, CommandLine FROM Win32_Process"))
                using (ManagementObjectCollection objects = searcher.Get())
                {
                    return objects.Cast<ManagementBaseObject>().ToDictionary(
                        x => x.GetPropertyValue("ProcessId")?.ToString(),
                        x => x.GetPropertyValue("CommandLine")?.ToString()
                        );
                }
            }
            catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
            {
                // Intentionally empty - no security access to the process.
                return null;
            }
            catch (InvalidOperationException)
            {
                // Intentionally empty - the process exited before getting details.
                return null;
            }
        }

        // Este método usa librerías / métodos externos para evitar usar la consulta a ManagementObjectSearcher, ya que esta afecta el rendimiento
        private static string GetProcessOwner(Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                System.Security.Principal.WindowsIdentity wi = new System.Security.Principal.WindowsIdentity(processHandle);
                string owner = wi.Name;
                return owner;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static string GetProcessDescriptiveName(Process process)
        {
            string name = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(process.MainModule.FileVersionInfo.FileDescription))
                    name = process.MainModule.FileVersionInfo.FileDescription;

                else if (!string.IsNullOrEmpty(process.MainWindowTitle))
                    name = process.MainWindowTitle;

                else if (!string.IsNullOrEmpty(process.MainModule.FileVersionInfo.InternalName)
                        && !string.IsNullOrEmpty(process.MainModule.FileVersionInfo.ProductName))
                    name = process.MainModule.FileVersionInfo.ProductName + " (" + process.MainModule.FileVersionInfo.InternalName + ")";

                else if (!string.IsNullOrEmpty(process.MainModule.FileVersionInfo.InternalName))
                    name = process.MainModule.FileVersionInfo.InternalName;

                else
                    name = process.ProcessName;

                return name;
            }
            catch (Exception)
            {
                return name;
            }
        }

        private static bool DoesProcessHaveWindows(Process process)
        {
            if (process == null)
                return false;

            if (process.MainWindowHandle != null && process.MainWindowHandle != IntPtr.Zero)
            {
                return true;
            }
            else
                return false;
        }

        // Get current processes, discard system processes and processes that are not accessible or that have exited, 
        // and may throw exception when trying to access their properties
        private static IEnumerable<Process> GetCurrentSessionProcessesWithActiveWindows()
        {
            int currentSessionID = Process.GetCurrentProcess().SessionId;

            return Process.GetProcesses()
                .Where(
                    x =>
                    !x.ProcessName.Equals("System")
                    && !x.ProcessName.Equals("Idle") // Descartar estos procesos
                    && x.SessionId == currentSessionID // Solo procesos de la sesión actual
                    && DoesProcessHaveWindows(x) // Solo procesos con ventanas activas
                );
        }
    }
}
