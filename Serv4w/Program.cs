using PluginHandler;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Serv4w
{
    public partial class Program
    {
        static string build = "Build 1";
        static string defaultDownloadLocationFormat = "{0} {1}"; // pluginDictionary[requestedPlugin].Name, Guid.NewGuid()
        static string pluginsFolder = "Plugins";
        static Dictionary<string, IPlugin> pluginDictionary = new Dictionary<string, IPlugin>();
        static Dictionary<string, string> pluginList = new Dictionary<string, string>();
        static ICollection<IPlugin> plugins;

        static void Main(string[] args)
        {
            checkForUpdates();
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = "Serv4w [" + build + "]";

            // Show list of available plugins and their numbers
            string requestedPlugin = getRequestedPlugin();

            // Show information about plugin to confirm it is the wanted one
            if (!confirmPluginSelection(requestedPlugin)) { Console.WriteLine("\nExiting"); Environment.Exit(2); }

            // Set console title to show running plugin
            Console.Title = "Serv4w [" + build + "] | Running " + requestedPlugin;
            
            // Get wanted install directory
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\nInstall destination [{0} {1}]:  ", pluginDictionary[requestedPlugin].Name, Guid.NewGuid());
            Console.ForegroundColor = ConsoleColor.White;
            string installationDirectory = Console.ReadLine();
            if (String.IsNullOrWhiteSpace(installationDirectory)) { installationDirectory = String.Format(defaultDownloadLocationFormat, pluginDictionary[requestedPlugin].Name, Guid.NewGuid()); }
            Console.WriteLine();

            // Remove illegal characters if any from installation directory
            foreach (char c in Path.GetInvalidPathChars()) { installationDirectory = installationDirectory.Replace(c, '_'); }
            
            // Check if requested destination is available, if it isn't use the default
            if (Directory.Exists(installationDirectory)) { installationDirectory = String.Format(defaultDownloadLocationFormat, pluginDictionary[requestedPlugin].Name, Guid.NewGuid()); }
            
            // Check if requested folder name is possible, if it is create it else use the default name
            createFolder:
            try
            {
                Directory.CreateDirectory(installationDirectory);
            }
            catch
            {
                installationDirectory = String.Format(defaultDownloadLocationFormat, pluginDictionary[requestedPlugin].Name, Guid.NewGuid());
                goto createFolder;
            }

            // Set working directory to installation folder
            Directory.SetCurrentDirectory(installationDirectory);
            
            // Inform about installation
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Installing");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" \"{0}\" ", requestedPlugin);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("to directory");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" \"{0}\"\n", installationDirectory);

            // Run plugin
            runPlugin(requestedPlugin, installationDirectory);
            
            // Close program after installer done
            Environment.Exit(0);
        }

        static string getRequestedPlugin()
        {
            showHeader();
            loadPlugins();
            showPluginList();

            string installId;
            askForId:
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Input the plugin number to install: ");
            Console.ForegroundColor = ConsoleColor.White;
            installId = Console.ReadLine();
            if (!Regex.IsMatch(installId, @"^\d+$") || !pluginList.ContainsKey(installId))
            {
                if (installId == "0")
                    Environment.Exit(1);
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                Console.ForegroundColor = ConsoleColor.Red;
                clearCurrentLine();
                Console.WriteLine("Invalid selection: " + installId);
                clearCurrentLine();
                goto askForId;
            }
            return pluginList[installId];
        }
        static bool confirmPluginSelection(string requestedPlugin)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.Clear();
            showHeader();

            showInfo("Name", pluginDictionary[requestedPlugin].Name);
            showInfo("Author", pluginDictionary[requestedPlugin].Author);
            showInfo("Plugin version", pluginDictionary[requestedPlugin].Version);
            showInfo("Engine", pluginDictionary[requestedPlugin].Engine);
            showInfo("From SteamCMD", pluginDictionary[requestedPlugin].Steamcmd);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Continue with installation? [Y/n]: ");
            Console.ForegroundColor = ConsoleColor.White;
            string response = Console.ReadKey().KeyChar.ToString().ToLower();
            Console.WriteLine();

            Console.ForegroundColor = oldColor;
            if (response == "n")
                return false;
            else
                return true;
        }

        static void checkForUpdates()
        {
            int thisBuild = Convert.ToInt32(build.Split(' ')[1]);
            int latestBuild = 1;

            using (WebClient wc = new WebClient())
                latestBuild = Convert.ToInt32(wc.DownloadString("https://github.com/Scarsz/Serv4w/raw/master/LatestBuild"));

            if (thisBuild < latestBuild)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This version of Serv4w is outdated.\nUpdate at https://github.com/Scarsz/Serv4w\n");
                Console.ForegroundColor = oldColor;
            }
        }
        static void clearCurrentLine()
        {
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write(" ");
            }
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
        static void showHeader()
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Serv4w");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("A game server installer suite for Windows");
            showInfo("License", "MIT (C) Scarsz (@ScarszRawr)");
            showInfo("Build #", build);
            Console.ForegroundColor = oldColor;
            Console.WriteLine();
        }
        static void showPluginList()
        {
            int id = 1;
            ConsoleColor oldColor = Console.ForegroundColor;
            foreach (var item in plugins)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(id);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("] ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(item.Name);
                //Console.WriteLine("[{0}] {1}", id, item.Name);
                id++;
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("0");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Exit");
            Console.ForegroundColor = oldColor;
            Console.WriteLine();
        }
        static void showInfo(string title, object info)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(title);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(info);
        }

        static void loadPlugins()
        {
            plugins = PluginLoader<IPlugin>.LoadPlugins(pluginsFolder);
            pluginList.Clear();

            int id = 1;
            foreach (var item in plugins)
            {
                if (!pluginDictionary.ContainsKey(item.Name))
                {
                    pluginDictionary.Add(item.Name, item);
                    pluginList.Add(id.ToString(), item.Name);
                    id++;
                }
            }
        }
        static void runPlugin(string pluginName, string installDestination)
        {
            if (String.IsNullOrWhiteSpace(pluginName))
                return;
            if (pluginDictionary.ContainsKey(pluginName))
            {
                IPlugin plugin = pluginDictionary[pluginName];
                plugin.Main(installDestination);
            }
        }
    }
}
