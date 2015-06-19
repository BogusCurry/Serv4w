using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace PluginHandler
{
    public class PluginTools
    {
        /// <summary>
        /// Download a file from the specified URL to the specified destination, verbosely
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="destination">File name to save the download to</param>
        public static void DownloadFile(string url, string destination)
        {
            Stopwatch sw = new Stopwatch();
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Downloading");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" \"{0}\" ", destination);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("from URL");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" \"{0}\"", url);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("...");
            sw.Restart();
            using (WebClient wc = new WebClient())
                wc.DownloadFile(url, destination);
            sw.Stop();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("done in {0} seconds\n", sw.ElapsedMilliseconds / 1000);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Execute the specified filename with the specified arguments
        /// </summary>
        /// <param name="filename">Filename to start</param>
        /// <param name="arguments">Arguments to pass to file</param>
        /// <param name="waitForExit">Whether or not to wait for the program to close before continuing</param>
        public static void Execute(string filename, string arguments = "", bool waitForExit = true)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Executing");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" {0} {1}\n\n", filename, arguments);
            Console.ForegroundColor = oldColor;
            Process process = Process.Start(new ProcessStartInfo(filename, arguments));
            if (waitForExit)
                process.WaitForExit();
        }

        /// <summary>
        /// Extracts the specified zip file to the specified path
        /// </summary>
        /// <param name="zippath">Path to zip file</param>
        /// <param name="extractpath">Path to extract zip file to</param>
        public static void ExtractZip(string zippath, string extractpath)
        {
            Directory.CreateDirectory(extractpath);
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Extracting zip file");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" {0} ", zippath);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("to");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" {0}\n", extractpath);
            Console.ForegroundColor = oldColor;
            //using (ZipArchive archive = ZipFile.OpenRead(zippath))
            //    foreach (ZipArchiveEntry entry in archive.Entries)
            //    {
            //        entry.ExtractToFile(Path.Combine(extractpath, entry.FullName));
            //    }
            ZipFile.ExtractToDirectory(zippath, extractpath);
        }

        /// <summary>
        /// Gets the latest Spigot server URL as well as the filename in the format "http|craftbukkit_server.jar"
        /// </summary>
        /// <returns></returns>
        public static string GetLatestBukkitServerJar()
        {
            return "http://getspigot.org/spigot18/craftbukkit_server.jar|craftbukkit_server.jar";
        }
        /// <summary>
        /// Gets the latest vanilla Minecraft server URL as well as the filename in the format "http|minecraft_server.1.5.jar"
        /// </summary>
        /// <returns></returns>
        public static string GetLatestMinecraftServerJar()
        {
            // Parsing HTML is hard
            string url = String.Empty;
            string filename = String.Empty;
            string[] beforeParse;
            using (WebClient wc = new WebClient())
                beforeParse = wc.DownloadString("https://mcversions.net/").Split(new string[] { "href=" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string segment in beforeParse)
                if (segment.Contains("\"https://s3.amazonaws.com/Minecraft.Download/versions"))
                    if (segment.Contains("server"))
                    {
                        url = segment.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        filename = url.Split('/').Last();
                        break;
                    }
            return url + "|" + filename;
        }
        /// <summary>
        /// Gets the latest Spigot server URL as well as the filename in the format "http|spigot_server.jar"
        /// </summary>
        /// <returns></returns>
        public static string GetLatestSpigotServerJar()
        {
            return "http://getspigot.org/spigot18/spigot_server.jar|spigot_server.jar";
        }
        /// <summary>
        /// Gets the latest TShock server URL as well as the filename in the format "http|tshock_4.2.10.zip
        /// </summary>
        /// <returns></returns>
        public static string GetLatestTshockServer()
        {
            string url = String.Empty;
            string filename = String.Empty;
            string[] beforeParse;
            using (WebClient wc = new WebClient())
                beforeParse = wc.DownloadString("https://github.com/NyxStudios/TShock/releases/latest").Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string segment in beforeParse)
                if (segment.Contains("href=\"/NyxStudios/TShock/releases/download/v")) // <a href="/NyxStudios/TShock/releases/download/v4.2.10/tshock_4.2.10.zip" rel="nofollow">
                    if (segment.Contains(".zip"))
                    {
                        url = "https://github.com" + segment.Split('\"')[1]; // https://github.com/NyxStudios/TShock/releases/download/v4.2.10/tshock_4.2.10.zip
                        filename = url.Split('/').Last();
                        break;
                    }
            return url + "|" + filename;
        }

        /// <summary>
        /// Installs SteamCMD to directory "SteamCMD"
        /// </summary>
        public static void InstallSteamcmd()
        {
            if (IsSteamcmdInstalled())
                return;
            DownloadFile("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip", "steamcmd.zip");
            ExtractZip("steamcmd.zip", "SteamCMD");
            File.Delete("steamcmd.zip");
            Execute(@"SteamCMD\steamcmd.exe", "+exit");
        }
        /// <summary>
        /// Returns a boolean for if SteamCMD\steamcmd.exe exists or not.
        /// </summary>
        /// <returns></returns>
        public static bool IsSteamcmdInstalled()
        {
            if (File.Exists(@"SteamCMD\steamcmd.exe"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Appends the specified text to the specified file
        /// </summary>
        /// <param name="file">File to append to</param>
        /// <param name="text">Text to append</param>
        public static void WriteTextToFile(string file, string text, bool overwrite = false)
        {
            if (overwrite)
                if (File.Exists(file))
                    File.Delete(file);
            using (StreamWriter sw = File.AppendText(file))
                sw.WriteLine(text);
        }
    }
}
