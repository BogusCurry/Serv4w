namespace PluginHandler
{
    public interface IPlugin
    {
        string Name { get; }
        string Author { get; }
        string Version { get; }
        string Engine { get; }
        bool Steamcmd { get; }
        string Appid { get; }
        void Main(string installDestination);
    }
}