using Exiled.API.Features;
using Exiled.API.Interfaces;
using System.ComponentModel;
using System.IO;

namespace MapImageGen
{
    public class Config : IConfig
    {
        // Plugin Settings

        [Description("Enable this plugin?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enable debugging logs.")]
        public bool Debug { get; set; } = false;

        [Description("Scale factor for image generation.")]
        public int ScaleFactor { get; set; } = 20;

    }
}