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
                DFAResultsObject.WaitList = this.DFACore.WaitList;
                DFAResultsObject.WaitTime = this.DFACore.WaitTime;
                DFAResultsObject.QueuedPartyStatus = new DFAResultsObject.PartyStatus()
                {
                    Tank = this.DFACore.QueuedTank,
                    Healer = this.DFACore.QueuedHealer,
                    Dps = this.DFACore.QueuedDps,
                    TankMax = this.DFACore.QueuedTankMax,
                    HealerMax = this.DFACore.QueuedHealerMax,
                    DpsMax = this.DFACore.QueuedDpsMax,
                };
                DFAResultsObject.MatchedPartyStatus = new DFAResultsObject.PartyStatus()
                {
                    Tank = this.DFACore.MatchedTank,
                    Healer = this.DFACore.MatchedHealer,
                    Dps = this.DFACore.MatchedDps,
                    TankMax = this.DFACore.MatchedTankMax,
                    HealerMax = this.DFACore.MatchedHealerMax,
                    DpsMax = this.DFACore.MatchedDpsMax,
                };
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
            public uint WaitList { get; set; }
            public uint WaitTime { get; set; }

            public PartyStatus QueuedPartyStatus { get; set; } = new PartyStatus();
            public PartyStatus MatchedPartyStatus { get; set; } = new PartyStatus();

            public class PartyStatus
            {
                public uint Tank { get; set; } = 0;
                public uint Healer { get; set; } = 0;
                public uint Dps { get; set; } = 0;

                public uint TankMax { get; set; } = 0;
                public uint HealerMax { get; set; } = 0;
                public uint DpsMax { get; set; } = 0;

            }
        }
    }
}
