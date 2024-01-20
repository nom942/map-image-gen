using System;
using Exiled.API.Features;
using Player = Exiled.Events.Handlers.Player;
using HarmonyLib;

namespace MapImageGen
{
    public class Plugin : Plugin<Config>
    {
        private EventHandler EventHandler;

        public override string Name => "MapImageGen";
        public override string Author => "Nom";
        public override Version Version => new Version(1, 0, 0);
        public static Plugin Instance { get; private set; }

        public int _patchesCounter;
        private Harmony harmony;

        public override void OnEnabled()
        {
            EventHandler = new EventHandler();

            Instance = this;

            try
            {
                harmony = new Harmony($"com.nom.patch");
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
      
            EventHandler = null;
            Instance = null;
            base.OnDisabled();
        }
    }
}
