namespace Qitana.DFAPlugin
{
    partial class DFAOverlayConfigPanel
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DFAOverlayConfigPanel));
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.label_ShowOverlay = new System.Windows.Forms.Label();
            this.checkDFAVisible = new System.Windows.Forms.CheckBox();
            this.label_Clickthru = new System.Windows.Forms.Label();
            this.checkDFAClickThru = new System.Windows.Forms.CheckBox();
            this.label_LockOverlay = new System.Windows.Forms.Label();
            this.checkDFALock = new System.Windows.Forms.CheckBox();
            this.label_URL = new System.Windows.Forms.Label();
            this.table_URL = new System.Windows.Forms.TableLayoutPanel();
            this.textDFAUrl = new System.Windows.Forms.TextBox();
            this.buttonDFASelectFile = new System.Windows.Forms.Button();
            this.label_DFAInterval = new System.Windows.Forms.Label();
            this.nudDFAInterval = new System.Windows.Forms.NumericUpDown();
            this.label_Hotkey = new System.Windows.Forms.Label();
            this.table_Hotkey = new System.Windows.Forms.TableLayoutPanel();
            this.checkDFAEnableGlobalHotkey = new System.Windows.Forms.CheckBox();
            this.textDFAGlobalHotkey = new System.Windows.Forms.TextBox();
            this.label_Framerate = new System.Windows.Forms.Label();
            this.nudDFAMaxFrameRate = new System.Windows.Forms.NumericUpDown();
            this.label_Help = new System.Windows.Forms.Label();
            this.panel_Buttons = new System.Windows.Forms.Panel();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonDFACopyActXiv = new System.Windows.Forms.Button();
            this.buttonDFAReloadBrowser = new System.Windows.Forms.Button();
            this.label_TTS = new System.Windows.Forms.Label();
            this.textBox_TTS = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel7.SuspendLayout();
            this.table_URL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDFAInterval)).BeginInit();
            this.table_Hotkey.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDFAMaxFrameRate)).BeginInit();
            this.panel_Buttons.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel7
            // 
            resources.ApplyResources(this.tableLayoutPanel7, "tableLayoutPanel7");
            this.tableLayoutPanel7.Controls.Add(this.label_ShowOverlay, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.checkDFAVisible, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.label_Clickthru, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.checkDFAClickThru, 1, 1);
            this.tableLayoutPanel7.Controls.Add(this.label_LockOverlay, 0, 2);
            this.tableLayoutPanel7.Controls.Add(this.checkDFALock, 1, 2);
            this.tableLayoutPanel7.Controls.Add(this.label_URL, 0, 3);
            this.tableLayoutPanel7.Controls.Add(this.table_URL, 1, 3);
            this.tableLayoutPanel7.Controls.Add(this.label_DFAInterval, 0, 4);
            this.tableLayoutPanel7.Controls.Add(this.nudDFAInterval, 1, 4);
            this.tableLayoutPanel7.Controls.Add(this.label_Hotkey, 0, 5);
            this.tableLayoutPanel7.Controls.Add(this.table_Hotkey, 1, 5);
            this.tableLayoutPanel7.Controls.Add(this.label_Framerate, 0, 6);
            this.tableLayoutPanel7.Controls.Add(this.nudDFAMaxFrameRate, 1, 6);
            this.tableLayoutPanel7.Controls.Add(this.label_Help, 0, 8);
            this.tableLayoutPanel7.Controls.Add(this.panel_Buttons, 1, 9);
            this.tableLayoutPanel7.Controls.Add(this.label_TTS, 0, 7);
            this.tableLayoutPanel7.Controls.Add(this.textBox_TTS, 1, 7);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            // 
            // label_ShowOverlay
            // 
            resources.ApplyResources(this.label_ShowOverlay, "label_ShowOverlay");
            this.label_ShowOverlay.Name = "label_ShowOverlay";
            // 
            // checkDFAVisible
            // 
            resources.ApplyResources(this.checkDFAVisible, "checkDFAVisible");
            this.checkDFAVisible.Name = "checkDFAVisible";
            this.checkDFAVisible.UseVisualStyleBackColor = true;
            this.checkDFAVisible.CheckedChanged += new System.EventHandler(this.checkDFAVisible_CheckedChanged);
            // 
            // label_Clickthru
            // 
            resources.ApplyResources(this.label_Clickthru, "label_Clickthru");
            this.label_Clickthru.Name = "label_Clickthru";
            // 
            // checkDFAClickThru
            // 
            resources.ApplyResources(this.checkDFAClickThru, "checkDFAClickThru");
            this.checkDFAClickThru.Name = "checkDFAClickThru";
            this.checkDFAClickThru.UseVisualStyleBackColor = true;
            this.checkDFAClickThru.CheckedChanged += new System.EventHandler(this.checkDFAClickThru_CheckedChanged);
            // 
            // label_LockOverlay
            // 
            resources.ApplyResources(this.label_LockOverlay, "label_LockOverlay");
            this.label_LockOverlay.Name = "label_LockOverlay";
            // 
            // checkDFALock
            // 
            resources.ApplyResources(this.checkDFALock, "checkDFALock");
            this.checkDFALock.Name = "checkDFALock";
            this.checkDFALock.UseVisualStyleBackColor = true;
            this.checkDFALock.CheckedChanged += new System.EventHandler(this.checkDFALock_CheckedChanged);
            // 
            // label_URL
            // 
            resources.ApplyResources(this.label_URL, "label_URL");
            this.label_URL.Name = "label_URL";
            // 
            // table_URL
            // 
            resources.ApplyResources(this.table_URL, "table_URL");
            this.table_URL.Controls.Add(this.textDFAUrl, 0, 0);
            this.table_URL.Controls.Add(this.buttonDFASelectFile, 1, 0);
            this.table_URL.Name = "table_URL";
            // 
            // textDFAUrl
            // 
            resources.ApplyResources(this.textDFAUrl, "textDFAUrl");
            this.textDFAUrl.Name = "textDFAUrl";
            this.textDFAUrl.TextChanged += new System.EventHandler(this.textDFAUrl_TextChanged);
            // 
            // buttonDFASelectFile
            // 
            resources.ApplyResources(this.buttonDFASelectFile, "buttonDFASelectFile");
            this.buttonDFASelectFile.Name = "buttonDFASelectFile";
            this.buttonDFASelectFile.UseVisualStyleBackColor = true;
            this.buttonDFASelectFile.Click += new System.EventHandler(this.buttonDFASelectFile_Click);
            // 
            // label_DFAInterval
            // 
            resources.ApplyResources(this.label_DFAInterval, "label_DFAInterval");
            this.label_DFAInterval.Name = "label_DFAInterval";
            // 
            // nudDFAInterval
            // 
            resources.ApplyResources(this.nudDFAInterval, "nudDFAInterval");
            this.nudDFAInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudDFAInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudDFAInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudDFAInterval.Name = "nudDFAInterval";
            this.nudDFAInterval.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudDFAInterval.ValueChanged += new System.EventHandler(this.nudDFAInterval_ValueChanged);
            // 
            // label_Hotkey
            // 
            resources.ApplyResources(this.label_Hotkey, "label_Hotkey");
            this.label_Hotkey.Name = "label_Hotkey";
            // 
            // table_Hotkey
            // 
            resources.ApplyResources(this.table_Hotkey, "table_Hotkey");
            this.table_Hotkey.Controls.Add(this.checkDFAEnableGlobalHotkey, 0, 0);
            this.table_Hotkey.Controls.Add(this.textDFAGlobalHotkey, 1, 0);
            this.table_Hotkey.Name = "table_Hotkey";
            // 
            // checkDFAEnableGlobalHotkey
            // 
            resources.ApplyResources(this.checkDFAEnableGlobalHotkey, "checkDFAEnableGlobalHotkey");
            this.checkDFAEnableGlobalHotkey.Name = "checkDFAEnableGlobalHotkey";
            this.checkDFAEnableGlobalHotkey.UseVisualStyleBackColor = true;
            this.checkDFAEnableGlobalHotkey.CheckedChanged += new System.EventHandler(this.checkDFAEnableGlobalHotkey_CheckedChanged);
            // 
            // textDFAGlobalHotkey
            // 
            resources.ApplyResources(this.textDFAGlobalHotkey, "textDFAGlobalHotkey");
            this.textDFAGlobalHotkey.Name = "textDFAGlobalHotkey";
            this.textDFAGlobalHotkey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textDFAGlobalHotkey_KeyDown);
            // 
            // label_Framerate
            // 
            resources.ApplyResources(this.label_Framerate, "label_Framerate");
            this.label_Framerate.Name = "label_Framerate";
            // 
            // nudDFAMaxFrameRate
            // 
            resources.ApplyResources(this.nudDFAMaxFrameRate, "nudDFAMaxFrameRate");
            this.nudDFAMaxFrameRate.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudDFAMaxFrameRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDFAMaxFrameRate.Name = "nudDFAMaxFrameRate";
            this.nudDFAMaxFrameRate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudDFAMaxFrameRate.ValueChanged += new System.EventHandler(this.nudDFAMaxFrameRate_ValueChanged);
            // 
            // label_Help
            // 
            resources.ApplyResources(this.label_Help, "label_Help");
            this.tableLayoutPanel7.SetColumnSpan(this.label_Help, 2);
            this.label_Help.Name = "label_Help";
            // 
            // panel_Buttons
            // 
            this.panel_Buttons.Controls.Add(this.tableLayoutPanel8);
            resources.ApplyResources(this.panel_Buttons, "panel_Buttons");
            this.panel_Buttons.Name = "panel_Buttons";
            // 
            // tableLayoutPanel8
            // 
            resources.ApplyResources(this.tableLayoutPanel8, "tableLayoutPanel8");
            this.tableLayoutPanel8.Controls.Add(this.buttonDFACopyActXiv, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.buttonDFAReloadBrowser, 1, 0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            // 
            // buttonDFACopyActXiv
            // 
            resources.ApplyResources(this.buttonDFACopyActXiv, "buttonDFACopyActXiv");
            this.buttonDFACopyActXiv.Name = "buttonDFACopyActXiv";
            this.buttonDFACopyActXiv.UseVisualStyleBackColor = true;
            this.buttonDFACopyActXiv.Click += new System.EventHandler(this.buttonDFACopyActXiv_Click);
            // 
            // buttonDFAReloadBrowser
            // 
            resources.ApplyResources(this.buttonDFAReloadBrowser, "buttonDFAReloadBrowser");
            this.buttonDFAReloadBrowser.Name = "buttonDFAReloadBrowser";
            this.buttonDFAReloadBrowser.UseVisualStyleBackColor = true;
            this.buttonDFAReloadBrowser.Click += new System.EventHandler(this.buttonDFAReloadBrowser_Click);
            // 
            // label_TTS
            // 
            resources.ApplyResources(this.label_TTS, "label_TTS");
            this.label_TTS.Name = "label_TTS";
            // 
            // textBox_TTS
            // 
            resources.ApplyResources(this.textBox_TTS, "textBox_TTS");
            this.textBox_TTS.Name = "textBox_TTS";
            this.textBox_TTS.TextChanged += new System.EventHandler(this.textBox_TTS_TextChanged);
            // 
            // DFAOverlayConfigPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel7);
            this.Name = "DFAOverlayConfigPanel";
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.table_URL.ResumeLayout(false);
            this.table_URL.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDFAInterval)).EndInit();
            this.table_Hotkey.ResumeLayout(false);
            this.table_Hotkey.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDFAMaxFrameRate)).EndInit();
            this.panel_Buttons.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Label label_Help;
        private System.Windows.Forms.Label label_Framerate;
        private System.Windows.Forms.NumericUpDown nudDFAMaxFrameRate;
        private System.Windows.Forms.Label label_Clickthru;
        private System.Windows.Forms.Label label_ShowOverlay;
        private System.Windows.Forms.Label label_URL;
        private System.Windows.Forms.CheckBox checkDFAVisible;
        private System.Windows.Forms.CheckBox checkDFAClickThru;
        private System.Windows.Forms.Panel panel_Buttons;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.Button buttonDFAReloadBrowser;
        private System.Windows.Forms.Button buttonDFACopyActXiv;
        private System.Windows.Forms.TableLayoutPanel table_URL;
        private System.Windows.Forms.TextBox textDFAUrl;
        private System.Windows.Forms.Button buttonDFASelectFile;
        private System.Windows.Forms.Label label_DFAInterval;
        private System.Windows.Forms.NumericUpDown nudDFAInterval;
        private System.Windows.Forms.Label label_Hotkey;
        private System.Windows.Forms.CheckBox checkDFAEnableGlobalHotkey;
        private System.Windows.Forms.TextBox textDFAGlobalHotkey;
        private System.Windows.Forms.TableLayoutPanel table_Hotkey;
        private System.Windows.Forms.Label label_LockOverlay;
        private System.Windows.Forms.CheckBox checkDFALock;
        private System.Windows.Forms.Label label_TTS;
        private System.Windows.Forms.TextBox textBox_TTS;
    }
}
