using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Qitana.DFAPlugin
{
    public partial class DFATraceWindow : Form
    {

        private List<ushort> filteredOpcode = new List<ushort>();
        private List<ushort> currentFilterOpcode = new List<ushort>();

        public DFATraceWindow(List<ushort> currentFilterOpcode)
        {
            InitializeComponent();
            this.currentFilterOpcode = currentFilterOpcode;
        }

        public void HandleMessage(long epoch, byte[] message)
        {
            try
            {
                if (message.Length < 32)
                {
                    return;
                }

                var opcode = BitConverter.ToUInt16(message, 18);

                if (checkBox_Filter.Checked && !filteredOpcode.Contains(opcode))
                {
                    filteredOpcode.Add(opcode);
                    textBox_Filtered.AppendText(opcode.ToString("X4") + Environment.NewLine);
                }

                if (checkBox_CurrentFilter.Checked)
                {
                    if (!currentFilterOpcode.Contains(opcode))
                    {
                        return;
                    }
                }
                else
                {
                    if (filteredOpcode.Contains(opcode))
                    {
                        return;
                    }
                }

                string msg = "";
                var time = DateTimeOffset.FromUnixTimeMilliseconds(epoch).ToLocalTime().ToString("HH:mm:ss.fff");
                var data = message.Skip(32).Take(32).ToArray();
                msg += "---------------------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine;
                msg += time + " " + "(00)(01)(02)(03)(04)(05)(06)(07)(08)(09)(10)(11)(12)(13)(14)(15)(16)(17)(18)(19)(20)(21)(22)(23)(24)(25)(26)(27)(28)(29)(30)(31)" + Environment.NewLine;
                msg += "   " + " |0x" + opcode.ToString("X4") + "|  " + BitConverter.ToString(data).Replace("-", "--") + Environment.NewLine;

                msg += "    | " + opcode.ToString().PadLeft(5, ' ') + "|";
                for (int i = 0; i < data.Length; i++)
                {
                    int val = data[i];
                    msg += " " + val.ToString().PadLeft(3, ' ');
                }
                msg += Environment.NewLine;

                msg += "            ";
                for (int i = 0; i < (data.Length / 2); i++)
                {
                    var val = BitConverter.ToUInt16(data, i * 2);
                    msg += "  " + val.ToString().PadLeft(6, ' ');
                }
                msg += Environment.NewLine;

                msg += "            ";
                for (int i = 0; i < (data.Length / 4); i++)
                {
                    var val = BitConverter.ToUInt32(data, i * 4);
                    msg += "  " + val.ToString().PadLeft(14, ' ');
                }
                msg += Environment.NewLine;

                this.SuspendLayout();
                textBox_Messages.AppendText(msg);
                this.ResumeLayout();

            }
            catch
            { }
        }

        private void checkBox_Filter_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Checked)
            {
                checkBox.Text = "Stop Filter";
                checkBox.BackColor = Color.LightSkyBlue;
            }
            else
            {
                checkBox.Text = "Start Filter";
                checkBox.BackColor = SystemColors.Control;
            }
        }

        private void checkBox_CurrentFilter_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Checked)
            {
                checkBox.Text = "Stop Current Filter";
                checkBox.BackColor = Color.LightSkyBlue;

                this.checkBox_Filter.Checked = false;

                this.checkBox_Filter.Enabled = false;
                this.textBox_Filtered.Enabled = false;
                this.textBox_Filtered.BackColor = SystemColors.Control;
                this.button_ClearFilter.Enabled = false;
            }
            else
            {
                checkBox.Text = "Set Current Filter";
                checkBox.BackColor = SystemColors.Control;

                this.checkBox_Filter.Enabled = true;
                this.textBox_Filtered.Enabled = true;
                this.textBox_Filtered.BackColor = SystemColors.Window;
                this.button_ClearFilter.Enabled = true;
            }


        }

        private void button_ClearMessages_Click(object sender, EventArgs e)
        {
            textBox_Messages.Clear();
        }

        private void button_ClearFilter_Click(object sender, EventArgs e)
        {
            filteredOpcode.Clear();
            textBox_Filtered.Clear();
        }

        private void button_SaveMessage_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName = "TraceMessage-" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt",
                Filter = "テキストァイル(*.txt)|*.txt|すべてのファイル(*.*)|*.*",
            };
            string messages = this.textBox_Messages.Text;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.File.WriteAllText(saveFileDialog.FileName, messages, Encoding.UTF8);
                }
                catch
                { }
            }
        }

    }
}
