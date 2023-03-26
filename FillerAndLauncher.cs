using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Management;


class Program
{
    static void Main()
    {
        // Find segatools.ini file
        string iniPath = FindIniFile(Directory.GetCurrentDirectory());

        // If segatools.ini file is found, update its contents
        if (!string.IsNullOrEmpty(iniPath))
        {
            // Find amfs and Option folders
            string amfsPath = FindFolder(iniPath, "amfs");
            string optionPath = FindFolder(iniPath, "Option");

            // Check if folders were found
            if (!string.IsNullOrEmpty(amfsPath) && !string.IsNullOrEmpty(optionPath))
            {
                // Find IPv4 address of this computer
                string ipAddress = GetIpAddress();

                // Update segatools.ini file
                UpdateIniFile(iniPath, amfsPath, optionPath, ipAddress);

                // Create APPDATA folder
                string appDataPath = Path.Combine(Path.GetDirectoryName(iniPath), "APPDATA");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                // Start game and servers
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c start /min start.bat & start /min brokenithm_server.exe & start /min aqua.bat",
                    WorkingDirectory = Path.GetDirectoryName(iniPath),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                var process = Process.Start(startInfo);

                // Close processes
                process.CloseMainWindow();
                CloseProcesses();
            }
            else
            {
                Console.WriteLine("amfs or Option folder was not found.");
            }
        }
        else
        {
            Console.WriteLine("segatools.ini was not found.");
        }
    }



    static string FindIniFile(string folderPath)
    {
        string[] iniFiles = Directory.GetFiles(folderPath, "segatools.ini");
        if (iniFiles.Length > 0)
        {
            return iniFiles[0];
        }
        else
        {
            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string subFolder in subFolders)
            {
                string iniPath = FindIniFile(subFolder);
                if (!string.IsNullOrEmpty(iniPath))
                {
                    return iniPath;
                }
            }
        }

        return null;
    }
    static string FindFolder(string iniPath, string folderName)
    {
        string binFolder = Path.GetDirectoryName(Path.GetDirectoryName(iniPath));
        string folderPath = Path.Combine(binFolder, folderName);

        if (Directory.Exists(folderPath))
        {
            return folderPath;
        }
        else
        {
            return null;
        }
    }



    static string GetIpAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ipAddress in host.AddressList)
        {
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ipAddress.ToString();
            }
        }
        return null;
    }

    static void UpdateIniFile(string iniPath, string amfsPath, string optionPath, string ipAddress)
    {
        string iniContents = File.ReadAllText(iniPath);
        iniContents = Regex.Replace(iniContents, $"amfs=([^\\r\\n]*)", $"amfs={amfsPath}");
        iniContents = Regex.Replace(iniContents, $"option=([^\\r\\n]*)", $"option={optionPath}");
        iniContents = Regex.Replace(iniContents, $"default=([^\\r\\n]*)", $"default={ipAddress}");
        iniContents = Regex.Replace(iniContents, $"appdata=([^\\r\\n]*)", $"appdata={Path.GetDirectoryName(iniPath)}\\APPDATA");
        File.WriteAllText(iniPath, iniContents);
    }

    static void CloseProcesses()
    {
        // Close brokenithm_server.exe
        Process[] processes = Process.GetProcessesByName("brokenithm_server");
        foreach (Process process in processes)
        {
            process.Kill();
        }

        // Close start.bat and inject_64.exe
        processes = Process.GetProcessesByName("cmd");
        foreach (Process process in processes)
        {
            string cmdLine = GetCommandLine(process);
            if (cmdLine.Contains("start.bat") || cmdLine.Contains("inject_64.exe"))
            {
                process.Kill();
            }
        }

        // Close aqua.bat
        processes = Process.GetProcessesByName("aqua");
        foreach (Process process in processes)
        {
            process.Kill();
        }
    }

    static string GetCommandLine(Process process)
    {
        string cmdLine = "";
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
        {
            foreach (ManagementObject @object in searcher.Get())
            {
                cmdLine = @object["CommandLine"]?.ToString();
                break;
            }
        }
        return cmdLine;
    }


}
