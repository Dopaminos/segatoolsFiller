using System.Net;
using System.Text.RegularExpressions;

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
            }
            else
            {
                Console.WriteLine("amfs or Option folder not found.");
            }
        }
        else
        {
            Console.WriteLine("segatools.ini not found.");
        }
        Console.WriteLine("segatools.ini was successfully filled!");
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
}