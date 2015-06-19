using PluginHandler;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Plugin_Minecraft_Spigot
{
    public class Plugin : IPlugin
    {
        static readonly string pluginName = "Minecraft - Spigot";
        static readonly string pluginAuthor = "Scarsz";
        static readonly string pluginVersion = "1";
        static readonly string gameEngine = "Java";
        static readonly bool usesSteamcmd = false;
        static readonly string appId = String.Empty;

        public string Name { get { return pluginName; } }
        public string Author { get { return pluginAuthor; } }
        public string Version { get { return pluginVersion; } }
        public string Engine { get { return gameEngine; } }
        public bool Steamcmd { get { return usesSteamcmd; } }
        public string Appid { get { return appId; } }

        static readonly string defaultMinimumMemory = "256";
        static readonly string defaultMaximumMemory = "2048";

        public void Main(string installDestination)
        {
            // Download latest jar file
            string latestJarRaw = PluginTools.GetLatestSpigotServerJar();
            string latestJarUrl = latestJarRaw.Split('|')[0];
            string latestJarFile = latestJarRaw.Split('|')[1];
            PluginTools.DownloadFile(latestJarUrl, latestJarFile);

            // Accept the EULA
            PluginTools.WriteTextToFile("eula.txt", "eula=true");

            // Create start files
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Enter the maximum amount of memory in MB the server can use [2048]: ");
            Console.ForegroundColor = ConsoleColor.White;
            string maxMemory = Console.ReadLine();
            if (!Regex.IsMatch(maxMemory, @"^\d+$"))
                maxMemory = defaultMaximumMemory;
            // No automatic restart
            PluginTools.WriteTextToFile("Start.bat", String.Format("@echo off\ntitle Minecraft Server\njava -Xmx{0}m -Xms{1}m -jar {2} nogui\npause", maxMemory, defaultMinimumMemory, latestJarFile));
            // Automatic restart
            PluginTools.WriteTextToFile("Start and restart automatically after crash.bat", String.Format("@echo off\ntitle Minecraft Server\n:start\njava -Xmx{0}m -Xms{1}m -jar {2} nogui\ngoto start", maxMemory, defaultMinimumMemory, latestJarFile));

            // Start the server
            PluginTools.Execute("Start.bat", "", false);
        }
    }
}