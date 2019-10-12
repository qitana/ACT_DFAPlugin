using RainbowMage.OverlayPlugin;
using System;
using System.Reflection;

namespace Qitana.DFAPlugin
{
    public sealed class OverlayAddonMain : IOverlayAddon
    {
        // OverlayPluginのリソースフォルダ
        public static string ResourcesDirectory = String.Empty;
        public static string UpdateMessage = String.Empty;

        public OverlayAddonMain()
        {
            // OverlayPlugin.Coreを期待
            Assembly asm = System.Reflection.Assembly.GetCallingAssembly();
            if (asm.Location == null || asm.Location == "")
            {
                // 場所がわからないなら自分の場所にする
                asm = Assembly.GetExecutingAssembly();
            }
            ResourcesDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "resources");
        }

        static OverlayAddonMain()
        {
            // static constructor should be called only once
            //UpdateMessage = UpdateChecker.Check();
            UpdateMessage = "UpdateChecker is Disabled.";
        }

        public string Name => "DFA";

        public string Description => "Show DFA statistics of current server.";

        public Type OverlayType => typeof(DFAOverlay);

        public Type OverlayConfigType => typeof(DFAOverlayConfig);

        public Type OverlayConfigControlType => typeof(DFAOverlayConfigPanel);

        public IOverlay CreateOverlayInstance(IOverlayConfig config) => new DFAOverlay((DFAOverlayConfig)config);

        public IOverlayConfig CreateOverlayConfigInstance(string name) => new DFAOverlayConfig(name);

        public System.Windows.Forms.Control CreateOverlayConfigControlInstance(IOverlay overlay) => new DFAOverlayConfigPanel((DFAOverlay)overlay);

        public void Dispose() { }
    }
}