using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Script.Serialization;
using RainbowMage.OverlayPlugin;

namespace Qitana.DFAPlugin
{
    [Serializable()]

    public class DFAOverlay : OverlayBase<DFAOverlayConfig>
    {
        private DFACore DFACore = null;
        private bool suppress_log = false;
        private bool isDebug = false;
        private object _lock = new object();

        public DFAOverlay(DFAOverlayConfig config) : base(config, config.Name)
        {
            if (config.Name.Equals("DFADebug"))
            {
                isDebug = true;
            }
        }

        public override void Dispose()
        {
            this.xivWindowTimer.Enabled = false;
            this.timer.Enabled = false;
            this.DFACore?.Dispose();
            base.Dispose();
        }

        /*
        public void ChangeProcessId(int processId)
        {
            lock (_lock)
            {
                Process p = null;

                if (Config.FollowFFXIVPlugin)
                {
                    if (FFXIVPluginHelper.Instance != null)
                    {
                        p = FFXIVPluginHelper.GetFFXIVProcess;
                    }
                }
                else
                {
                    p = FFXIVProcessHelper.GetFFXIVProcess(processId);
                }

                if ((DFAController == null && p != null) ||
                    (DFAController != null && p != null && p.Id != DFAController.Process.Id))
                {
                    try
                    {
                        DFAController = new DFAController(this, p);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message);
                        suppress_log = true;
                        DFAController = null;
                    }
                }
                else if (DFAController != null && p == null)
                {
                    DFAController.Dispose();
                    DFAController = null;
                }
            }
        }
        */
        public void LogDebug(string format, params object[] args)
        {
            string prefix = isDebug ? "DEBUG: " : "";
            LogLevel level = isDebug ? LogLevel.Info : LogLevel.Debug;
            Log(level, prefix + format, args);
        }

        public void LogError(string format, params object[] args)
        {
            if (suppress_log == false)
            {
                Log(LogLevel.Error, format, args);
            }
        }

        public void LogWarning(string format, params object[] args)
        {
            if (suppress_log == false)
            {
                Log(LogLevel.Warning, format, args);
            }
        }

        public void LogInfo(string format, params object[] args)
            => Log(LogLevel.Info, format, args);



        /*
        /// <summary>
        /// プロセスの有効性をチェック
        /// </summary>
        private void CheckProcessId()
        {
            try
            {
                if (Config.FollowFFXIVPlugin)
                {
                    Process p = null;
                    if (FFXIVPluginHelper.Instance != null)
                    {
                        p = FFXIVPluginHelper.GetFFXIVProcess;
                        if (p == null || (DFAController != null && DFAController.Process.Id != p.Id))
                        {
                            DFAController?.Dispose();
                            DFAController = null;
                        }
                    }
                }

                if (DFAController == null)
                {
                    ChangeProcessId(0);
                }
                else if (DFAController.ValidateProcess())
                {
                    // スキャン間隔をもどす
                    if (timer.Interval != this.Config.Interval)
                    {
                        timer.Interval = this.Config.Interval;
                    }

                    if (suppress_log == true)
                    {
                        suppress_log = false;
                    }
                }
                else
                {
                    DFAController?.Dispose();
                    DFAController = null;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }
        */


        protected override void Update()
        {
            try
            {
                if (this.DFACore == null)
                {
                    DFACore = new DFACore();
                    return;
                }

                if (!DFACore.IsAttached)
                {
                    DFACore.Attach();
                    return;
                }


                string updateScript = CreateEventDispatcherScript();
                if (this.Overlay != null &&
                    this.Overlay.Renderer != null &&
                    this.Overlay.Renderer.Browser != null)
                {
                    this.Overlay.Renderer.Browser.GetMainFrame().ExecuteJavaScript(updateScript, null, 0);
                }
            }
            catch (Exception ex)
            {
                LogError("Update: {0} {1}", this.Name, ex.ToString());
            }
        }

        /// <summary>
        /// データを取得し、JSONを作る
        /// </summary>
        /// <returns></returns>
        internal string CreateJsonData()
        {
            // シリアライザ
            var serializer = new JavaScriptSerializer();

            // Overlay に渡すオブジェクトを作成
            DFAResultsObject DFAResultsObject = new DFAResultsObject();

            // なんかプロセスがおかしいとき
            if (DFACore == null || DFACore.IsActive == false || DFACore.IsAttached == false)
            {
                return serializer.Serialize(DFAResultsObject);
            }

            try
            {
                DFAResultsObject.State = this.DFACore.State;
                DFAResultsObject.RouletteCode = this.DFACore.RouletteCode;
                DFAResultsObject.Code = this.DFACore.Code;
            }
            catch (Exception ex)
            {
                LogError("Update: {1}", this.Name, ex);
            }
            return serializer.Serialize(DFAResultsObject);
        }

        private string CreateEventDispatcherScript()
            => "var ActXiv = { 'DFAData': " + this.CreateJsonData() + " };\n" +
               "document.dispatchEvent(new CustomEvent('onOverlayDataUpdate', { detail: ActXiv }));";

        /// <summary>
        /// スキャン間隔を更新する
        /// </summary>
        public void UpdateScanInterval()
        {
            timer.Interval = this.Config.Interval;
            LogDebug(Messages.UpdateScanInterval, this.Config.Interval);
        }

        /// <summary>
        /// スキャンを開始する
        /// </summary>
        public new void Start()
        {
            if (OverlayAddonMain.UpdateMessage != String.Empty)
            {
                LogInfo(OverlayAddonMain.UpdateMessage);
                OverlayAddonMain.UpdateMessage = String.Empty;
            }
            if (this.Config.IsVisible == false)
            {
                return;
            }
            LogInfo(Messages.StartScanning);
            suppress_log = false;
            timer.Start();
        }

        /// <summary>
        /// スキャンを停止する
        /// </summary>
        public new void Stop()
        {
            if (timer.Enabled)
            {
                timer.Stop();
                LogInfo(Messages.StopScanning);
            }
        }

        protected override void InitializeTimer() => base.InitializeTimer();

        //// JSON用オブジェクト
        private class DFAResultsObject
        {
            public string State { get; set; }
            public int RouletteCode { get; set; }
            public int Code { get; set; }
        }
    }
}
