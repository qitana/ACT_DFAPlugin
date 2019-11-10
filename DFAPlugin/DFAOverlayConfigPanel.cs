using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using RainbowMage.OverlayPlugin;
using System.Diagnostics;

namespace Qitana.DFAPlugin
{
    public partial class DFAOverlayConfigPanel : UserControl
    {
        private DFAOverlay overlay;
        private DFAOverlayConfig config;

        public DFAOverlayConfigPanel(DFAOverlay overlay)
        {
            InitializeComponent();

            this.overlay = overlay;
            this.config = overlay.Config;

            SetupControlProperties();
            SetupConfigEventHandlers();
        }

        private void SetupControlProperties()
        {
            this.checkDFAVisible.Checked = this.config.IsVisible;
            this.checkDFAClickThru.Checked = this.config.IsClickThru;
            this.checkDFALock.Checked = this.config.IsLocked;
            this.textDFAUrl.Text = this.config.Url;
            this.nudDFAMaxFrameRate.Value = this.config.MaxFrameRate;
            this.nudDFAInterval.Value = this.config.Interval;
            this.checkDFAEnableGlobalHotkey.Checked = this.config.GlobalHotkeyEnabled;
            this.textDFAGlobalHotkey.Enabled = this.checkDFAEnableGlobalHotkey.Checked;
            this.textDFAGlobalHotkey.Text = GetHotkeyString(this.config.GlobalHotkeyModifiers, this.config.GlobalHotkey);
            this.textBox_TTS.Text = this.config.TTS;
        }

        private void SetupConfigEventHandlers()
        {
            this.config.VisibleChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.checkDFAVisible.Checked = e.IsVisible;
                });
            };
            this.config.ClickThruChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.checkDFAClickThru.Checked = e.IsClickThru;
                });
            };
            this.config.LockChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.checkDFALock.Checked = e.IsLocked;
                });
            };
            this.config.UrlChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.textDFAUrl.Text = e.NewUrl;
                });
            };
            this.config.MaxFrameRateChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.nudDFAMaxFrameRate.Value = e.NewFrameRate;
                });
            };
            this.config.IntervalChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.nudDFAInterval.Value = e.NewInterval;
                });
            };
            this.config.GlobalHotkeyEnabledChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.checkDFAEnableGlobalHotkey.Checked = e.NewGlobalHotkeyEnabled;
                    this.textDFAGlobalHotkey.Enabled = this.checkDFAEnableGlobalHotkey.Checked;
                });
            };
            this.config.GlobalHotkeyChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.textDFAGlobalHotkey.Text = GetHotkeyString(this.config.GlobalHotkeyModifiers, e.NewHotkey);
                });
            };
            this.config.GlobalHotkeyModifiersChanged += (o, e) =>
            {
                this.InvokeIfRequired(() =>
                {
                    this.textDFAGlobalHotkey.Text = GetHotkeyString(e.NewHotkey, this.config.GlobalHotkey);
                });
            };
        }

        private void InvokeIfRequired(Action action)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void checkDFAVisible_CheckedChanged(object sender, EventArgs e)
        {
            this.config.IsVisible = this.checkDFAVisible.Checked;
            if (this.overlay != null)
            {
                if (this.config.IsVisible == true) {
                    this.overlay.Start();
                }
                else
                {
                    this.overlay.Stop();
                }
            }
        }

        private void checkDFAClickThru_CheckedChanged(object sender, EventArgs e)
        {
            this.config.IsClickThru = this.checkDFAClickThru.Checked;
        }

        private void checkDFALock_CheckedChanged(object sender, EventArgs e)
        {
            this.config.IsLocked = this.checkDFALock.Checked;
        }

        private void textDFAUrl_TextChanged(object sender, EventArgs e)
        {
            this.config.Url = this.textDFAUrl.Text;
        }

        private void buttonDFASelectFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.config.Url = new Uri(ofd.FileName).ToString();
            }
        }

        private void buttonDFACopyActXiv_Click(object sender, EventArgs e)
        {
            var json = this.overlay.CreateJsonData();
            if (!string.IsNullOrWhiteSpace(json))
            {
                Clipboard.SetText("var ActXiv = { 'DFAData': " + json + " };\n");
            }
        }

        private void buttonDFAReloadBrowser_Click(object sender, EventArgs e)
        {
            this.overlay.Navigate(this.config.Url);
        }

        private void nudDFAMaxFrameRate_ValueChanged(object sender, EventArgs e)
        {
            this.config.MaxFrameRate = (int)nudDFAMaxFrameRate.Value;
        }

        private void nudDFAInterval_ValueChanged(object sender, EventArgs e)
        {
            this.config.Interval = (int)nudDFAInterval.Value;
            if (this.overlay != null)
            {
                this.overlay.UpdateScanInterval();
            }
        }
        
        private void textBox_TTS_TextChanged(object sender, EventArgs e)
        {
            this.config.TTS = this.textBox_TTS.Text;
            if (this.overlay != null)
            {
                this.overlay.UpdateTTS();
            }
        }

        private void checkDFAEnableGlobalHotkey_CheckedChanged(object sender, EventArgs e)
        {
            this.config.GlobalHotkeyEnabled = this.checkDFAEnableGlobalHotkey.Checked;
            this.textDFAGlobalHotkey.Enabled = this.config.GlobalHotkeyEnabled;
        }

        private void textDFAGlobalHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            var key = RemoveModifiers(e.KeyCode, e.Modifiers);
            this.config.GlobalHotkey = key;
            this.config.GlobalHotkeyModifiers = e.Modifiers;
        }

        /// <summary>
        ///   Generates human readable keypress string
        ///   人間が読めるキー押下文字列を生成します
        /// </summary>
        /// <param name="Modifier"></param>
        /// <param name="key"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        private string GetHotkeyString(Keys Modifier, Keys key, String defaultText = "")
        {
            StringBuilder sbKeys = new StringBuilder();
            if ((Modifier & Keys.Shift) == Keys.Shift)
            {
                sbKeys.Append("Shift + ");
            }
            if ((Modifier & Keys.Control) == Keys.Control)
            {
                sbKeys.Append("Ctrl + ");
            }
            if ((Modifier & Keys.Alt) == Keys.Alt)
            {
                sbKeys.Append("Alt + ");
            }
            if ((Modifier & Keys.LWin) == Keys.LWin || (Modifier & Keys.RWin) == Keys.RWin)
            {
                sbKeys.Append("Win + ");
            }
            sbKeys.Append(Enum.ToObject(typeof(Keys), key).ToString());
            return sbKeys.ToString();
        }

        /// <summary>
        ///  Removes stray references to Left/Right shifts, etc and modifications of the actual key value caused by bitwise operations
        ///  ビット単位の操作に起因する左/右シフト、などと実際のキー値の変更に浮遊の参照を削除します。
        /// </summary>
        /// <param name="KeyCode"></param>
        /// <param name="Modifiers"></param>
        /// <returns></returns>
        private Keys RemoveModifiers(Keys KeyCode, Keys Modifiers)
        {
            var key = KeyCode;
            var modifiers = new List<Keys>() { Keys.ControlKey, Keys.LControlKey, Keys.Alt, Keys.ShiftKey, Keys.Shift, Keys.LShiftKey, Keys.RShiftKey, Keys.Control, Keys.LWin, Keys.RWin };
            foreach (var mod in modifiers)
            {
                if (key.HasFlag(mod))
                {
                    if (key == mod)
                        key &= ~mod;
                }
            }
            return key;
        }
    }
}
