using PluginHandler;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Plugin_Terraria_TShock
{
    public class Plugin : IPlugin
    {
        static readonly string pluginName = "Terraria - TShock";
        static readonly string pluginAuthor = "Scarsz";
        static readonly string pluginVersion = "1";
        static readonly string gameEngine = "XNA";
        static readonly bool usesSteamcmd = false;
        static readonly string appId = String.Empty;

        public string Name { get { return pluginName; } }
        public string Author { get { return pluginAuthor; } }
        public string Version { get { return pluginVersion; } }
        public string Engine { get { return gameEngine; } }
        public bool Steamcmd { get { return usesSteamcmd; } }
        public string Appid { get { return appId; } }

        public void Main(string installDestination)
        {
            // Nothing to really do for this server

            // Download latest zip file
            string latestZipRaw = PluginTools.GetLatestTshockServer();
            string latestZipUrl = latestZipRaw.Split('|')[0];
            string latestZipFile = latestZipRaw.Split('|')[1];
            PluginTools.DownloadFile(latestZipUrl, latestZipFile);

            // Extract to TShock folder
            PluginTools.ExtractZip(latestZipFile, @".\");

            // Delete now un-needed TShock release zip
            File.Delete(latestZipFile);

            // Start the server
            PluginTools.Execute("TerrariaServer.exe", "", false);
        }
    }
}