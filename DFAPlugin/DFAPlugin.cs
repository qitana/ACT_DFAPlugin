using RainbowMage.OverlayPlugin;
using Advanced_Combat_Tracker;
using System.Windows.Forms;

namespace Qitana.DFAPlugin
{
    public class DFAPlugin : IActPluginV1, IOverlayAddonV2
    {
        public DFAPlugin()
        {
            UpdateChecker.Check();
        }

        public static string pluginPath = "";

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginStatusText.Text = "Ready.";

            // We don't need a tab here.
            ((TabControl)pluginScreenSpace.Parent).TabPages.Remove(pluginScreenSpace);

            foreach (var plugin in ActGlobals.oFormActMain.ActPlugins)
            {
                if (plugin.pluginObj == this)
                {
                    pluginPath = plugin.pluginFile.FullName;
                    break;
                }
            }
        }

        public void DeInitPlugin()
        {

        }

        public string Name => "DFA";

        public string Description => "Show DFA statistics of current server.";

        public void Init()
        {
            Registry.RegisterEventSource<DFAEventSource>();
        }
    }
}