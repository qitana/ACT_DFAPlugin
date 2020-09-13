using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Qitana.DFAPlugin
{
    public partial class DFATraceWindow : Form
    {

        private List<ushort> filteredOpcode = new List<ushort>();

        public DFATraceWindow()
        {
            InitializeComponent();
            this.textBox_Messages.AppendText(
                "**********************************************" + Environment.NewLine +
                "  Step.1 Push [Start Filter]." + Environment.NewLine +
                "  Step.2 Push [Resume Capture]." + Environment.NewLine +
                "  Step.3 Walk around town, change area." + Environment.NewLine +
                "  Step.4 Enter Inn Room." + Environment.NewLine +
                "  Step.5 Click [Stop Filter]." + Environment.NewLine +
                "  Step.6 Now, ready to search opcode!" + Environment.NewLine +
                "**********************************************" + Environment.NewLine);
        }

        public void HandleMessage(long epoch, byte[] message)
        {
            try
            {
                if (this.checkBox_SuspendCapture.Checked)
                {
                    return;
                }

                if (message.Length < 32)
                {
                    return;
                }

                var opcode = BitConverter.ToUInt16(message, 18);

                if (filteredOpcode.Contains(opcode))
                {
                    return;
                }

                var data = message.Skip(32).ToArray();

                if (checkBox_Filter.Checked)
                {
                    if (checkPreferedRoleData(data))
                    {
                        return;
                    }
                    else
                    {
                        filteredOpcode.Add(opcode);
                        textBox_Filtered.AppendText(opcode.ToString() + Environment.NewLine);
                        return;
                    }
                }

                var time = DateTimeOffset.FromUnixTimeMilliseconds(epoch).ToLocalTime().ToString("HH:mm:ss.fff");
                var opcodeDecoded = opcode.ToString() + ",0x" + opcode.ToString("X4");
                var data32 = data.Take(32).ToArray();
                var speculated = speculateOpcodeFromData(data);

                string msg = "";
                msg += "---------------------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine;
                msg += "*****  " + time + " | Opcode: " + opcodeDecoded + " | PktLen: " + message.Length.ToString() + " | DataLen: " + (message.Length - 32) + " | Name: " + speculated[0] + Environment.NewLine;
                msg += " " + speculated[1] + " | (00)(01)(02)(03)(04)(05)(06)(07)(08)(09)(10)(11)(12)(13)(14)(15)(16)(17)(18)(19)(20)(21)(22)(23)(24)(25)(26)(27)(28)(29)(30)(31)" + Environment.NewLine;
                msg += opcode.ToString().PadLeft(4, ' ') + " |  " + BitConverter.ToString(data32).Replace("-", "--") + Environment.NewLine;

                msg += "     |";
                for (int i = 0; i < data32.Length; i++)
                {
                    int val = data32[i];
                    msg += " " + val.ToString().PadLeft(3, ' ');
                }
                msg += Environment.NewLine;

                this.SuspendLayout();
                textBox_Messages.AppendText(msg);
                this.ResumeLayout();

            }
            catch
            { }
        }

        private string[] speculateOpcodeFromData(byte[] data)
        {

            // PreferredRole
            if (checkPreferedRoleData(data))
            {
                return new string[] { "PreferredRole", "PRF" };
            }
            // Started
            else if (data.Length == 32 && ((data[8] == 0 && data[12] > 0) || (data[8] > 0 && data[12] == 0)))
            {
                return new string[] { "Started", "STA" };
            }
            // Matched
            else if (data.Length == 32 && ((data[2] == 0 && data[20] > 0) || data[2] > 0 && data[20] == 0))
            {
                if (this.checkBox_Undersized.Checked)
                {
                    if (data[2] == 0 && data[20] > 0 && data[16] == 1 &&
                        data[25] == 0 && data[27] == 0 && data[29] == 0 && data[31] == 1)
                    {
                        return new string[] { "Matched", "MAT" };
                    }
                }
                else
                {
                    if ((data[25] == 1 && data[27] == 1 && data[29] == 2 && data[31] == 0) ||
                        (data[25] == 2 && data[27] == 2 && data[29] == 4 && data[31] == 0) ||
                        (data[25] == 3 && data[27] == 6 && data[29] == 15 && data[31] == 0))
                    {
                        return new string[] { "Matched", "MAT" };
                    }
                }
            }
            // PartyUpdate
            else if (data.Length == 24 && ((data[2] == 0 && data[8] > 0) || (data[2] > 0 && data[8] == 0)))
            {
                if (this.checkBox_Undersized.Checked)
                {
                    if (data[2] == 0 && data[8] > 0 && data[18] == 1 && data[19] == 1)
                    {
                        return new string[] { "PartyUpdate", "PTY" };
                    }
                }
                else
                {
                    if ((data[12] <= 1 && data[13] == 1 && data[14] <= 1 && data[15] == 1 && data[16] <= 2 && data[17] == 2) ||
                        (data[12] <= 2 && data[13] == 2 && data[14] <= 2 && data[15] == 2 && data[16] <= 4 && data[17] == 4) ||
                        (data[12] <= 3 && data[13] == 3 && data[14] <= 6 && data[15] == 6 && data[16] <= 15 && data[17] == 15))
                    {
                        return new string[] { "PartyUpdate", "PTY" };
                    }
                }

            }
            // WaitQueue (RoleFree)
            else if (data.Length == 16 && data[0] > 0 && this.checkBox_Undersized.Checked && data.Skip(6).Take(10).All(x => x == 0))
            {
                return new string[] { "WaitQueueRoleFree", "QUE" };
            }
            // WaitQueue
            else if (data.Length == 16 && data[0] > 0 && data[7] > 0)
            {
                if ((data.Skip(8).Take(8).All(x => x == 0)) || // roulette
                    (data[8] <= 1 && data[9] == 1 && data[10] <= 1 && data[11] == 1 && data[12] <= 2 && data[13] == 2) || // light-party
                    (data[8] <= 2 && data[9] == 2 && data[10] <= 2 && data[11] == 2 && data[12] <= 4 && data[13] == 4) || // full-party
                    (data[8] <= 3 && data[9] == 3 && data[10] <= 6 && data[11] == 6 && data[12] <= 15 && data[13] == 15)) // alliance
                {
                    return new string[] { "WaitQueue", "QUE" };
                }
            }
            // Completed
            else if (data.Length == 16 && data[0] > 0 && data.Skip(1).Take(3).All(x => x == 0))
            {
                return new string[] { "Completed", "CMP" };
            }
            // Canceled
            else if (data.Length == 8 && data[0] == 122)
            {
                return new string[] { "Canceled", "CXL" };
            }

            return new string[] { "None", "   " };

        }

        private bool checkPreferedRoleData(byte[] data)
        {
            if (data.Length == 16 &&
                data[0] == 0 && data.Skip(1).Take(10).All(x => x >= 1 && x <= 4))
            {
                return true;
            }

            return false;
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

        private void checkBox_SuspendCapture_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Checked)
            {
                checkBox.Text = "Resume Capture";
                checkBox.BackColor = Color.LightSkyBlue;
            }
            else
            {
                checkBox.Text = "Suspend Capture";
                checkBox.BackColor = SystemColors.Control;
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
