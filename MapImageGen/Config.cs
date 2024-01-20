using Exiled.API.Interfaces;
using System.ComponentModel;

namespace MapImageGen
{
    public class Config : IConfig
    {
        // Plugin Settings

        [Description("Enable this plugin?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enable debugging logs.")]
        public bool Debug { get; set; } = true;

        [Description("Scale factor for image generation.")]
        public int ScaleFactor { get; set; } = 25;

        [Description("Web server IP.")]
        public string WebServerIP { get; set; } = "";

    }
}