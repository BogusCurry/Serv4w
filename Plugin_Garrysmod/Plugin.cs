using PluginHandler;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Plugin_Garrysmod
{
    public class Plugin : IPlugin
    {
        static readonly string pluginName = "Garry's Mod";
        static readonly string pluginAuthor = "Scarsz";
        static readonly string pluginVersion = "1";
        static readonly string gameEngine = "Source";
        static readonly bool usesSteamcmd = true;
        static readonly string appId = "4020";

        public string Name { get { return pluginName; } }
        public string Author { get { return pluginAuthor; } }
        public string Version { get { return pluginVersion; } }
        public string Engine { get { return gameEngine; } }
        public bool Steamcmd { get { return usesSteamcmd; } }
        public string Appid { get { return appId; } }

        static readonly string topFolder = "garrysmod";
        static readonly string windowName = "Garry's Mod Server";
        static readonly string startCommand = "srcds.exe -console -game garrysmod +map gm_construct -maxplayers 16 +rcon_password password";
        static readonly string updateArguments = "+login anonymous +force_install_dir \"..\\" + topFolder + "\" +app_update " + appId + " validate +quit";
        static readonly string updateCommand = @"SteamCMD\steamcmd.exe " + updateArguments;

        public void Main(string installDestination)
        {
            // Install SteamCMD
            PluginTools.InstallSteamcmd();
            Console.ReadKey();
            // Install the server
            PluginTools.Execute(@"SteamCMD\steamcmd.exe", String.Format(updateArguments, installDestination));

            // Create batch files
            // No automatic restart
            PluginTools.WriteTextToFile("Start.bat", String.Format("@echo off\ntitle {0}\n{1}", windowName, "cd " + topFolder + " && start \"\"" + startCommand));
            // Automatic restart
            PluginTools.WriteTextToFile("Start and restart automatically after crash.bat", String.Format("@echo off\ntitle {0}\n{2}\n:start\n{1}\ngoto start", windowName, startCommand, "cd " + topFolder));
            // Update
            PluginTools.WriteTextToFile("Update.bat", String.Format("@echo off\ntitle {0}\n{1}", windowName, String.Format(updateCommand, topFolder)));
            // Update then start
            PluginTools.WriteTextToFile("Update then start.bat", String.Format("@echo off\nUpdate.bat\nStart.bat"));
            // Update then automatically restart
            PluginTools.WriteTextToFile("Update then start and restart automatically after crash.bat", String.Format("@echo off\nUpdate.bat && \"Start and restart automatically after crash.bat\""));

            // Start the server
            PluginTools.Execute("Start.bat", "", false);
        }
    }
}