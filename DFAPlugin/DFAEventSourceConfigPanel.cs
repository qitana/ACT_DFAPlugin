using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Qitana.DFAPlugin
{
    public partial class DFAEventSourceConfigPanel : UserControl
    {
        private DFAEventSource source;
        private DFAEventSourceConfig config;
        private DFATraceWindow traceWindow;
        private bool IsTraceWindowActive => this.traceWindow != null && !this.traceWindow.IsDisposed;

        public DFAEventSourceConfigPanel(DFAEventSource source)
        {
            InitializeComponent();
            this.source = source;
            this.config = source.Config;
            //traceWindow = source.TraceWindow;

            SetupControlProperties();

            source.ffxivPluginNetworkReceivedDelegate -= MessageReceived;
            source.ffxivPluginNetworkReceivedDelegate += MessageReceived;

        }

        private void SetupControlProperties()
        {
            this.textBox_StructuresURL.Text = config.StructuresURL;
        }

        private void MessageReceived(string connection, long epoch, byte[] message)
        {
            if (IsTraceWindowActive)
            {
                try
                {
                    this.traceWindow.HandleMessage(epoch, message);
                }
                catch
                { }
            }
        }
        private void textBox_StructuresURL_TextChanged(object sender, EventArgs e)
        {
            this.config.StructuresURL = this.textBox_StructuresURL.Text;
        }

        private void textBox_TTS_TextChanged(object sender, EventArgs e)
        {
            this.config.TextToSpeech = this.textBox_TTS.Text;

        }


        private void button_OpenTraceWindow_Click(object sender, EventArgs e)
        {
            if (IsTraceWindowActive)
            {
                this.traceWindow.Activate();
            }
            else
            {
                this.traceWindow = new DFATraceWindow();
                this.traceWindow.Show();

            }
        }

    }
}
