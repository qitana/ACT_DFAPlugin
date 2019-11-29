using Advanced_Combat_Tracker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RainbowMage.OverlayPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Qitana.DFAPlugin
{

    public class DFAEventSource : EventSourceBase, ILogger
    {
        public DFAEventSourceConfig Config { get; private set; }

        private System.Timers.Timer processCheckTimer;
        private System.Timers.Timer updateEventTimer;

        // Events
        public delegate void DFAStatusUpdateHandler(JSEvents.DFAStatusUpdateEvent e);
        public event DFAStatusUpdateHandler onDFAStatusUpdate;

        private IActPluginV1 ffxivPlugin;
        internal FFXIV_ACT_Plugin.Common.NetworkReceivedDelegate ffxivPluginNetworkReceivedDelegate;
        internal FFXIV_ACT_Plugin.Common.ZoneChangedDelegate ffxivPluginZoneChangedDelegate;
        private IEnumerable<ushort> opcodeList = new List<ushort>();
        private static HttpClient httpClient = new HttpClient(new WebRequestHandler()
        {
            CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore)
        })
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        private Status status = new Status();
        public class Status
        {
            [JsonIgnore]
            public object LockObject { get; set; } = new object();

            [JsonIgnore]
            public MatchingState MatchingState { get; set; } = MatchingState.IDLE;
            public string MatchingStateString => this.MatchingState.ToString();

            public ushort RouletteCode { get; set; }
            public ushort DungeonCode { get; set; }

            public ushort WaitList { get; set; }
            public ushort WaitTime { get; set; }

            public int Tank { get; set; }
            public int TankMax { get; set; }
            public int Healer { get; set; }
            public int HealerMax { get; set; }
            public int Dps { get; set; }
            public int DpsMax { get; set; }

            public void Clear()
            {
                this.MatchingState = MatchingState.IDLE;
                SetZero();
            }
            public void SetZero()
            {
                this.RouletteCode = 0;
                this.DungeonCode = 0;
                this.WaitList = 0;
                this.WaitTime = 0;
                this.Tank = 0;
                this.TankMax = 0;
                this.Healer = 0;
                this.HealerMax = 0;
                this.Dps = 0;
                this.DpsMax = 0;
            }

            public string ToJson()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
        public enum MatchingState : int
        {
            IDLE = 0,
            QUEUED = 1,
            MATCHED = 2,
        }

        public bool IsDataSubscriptionHandled { get; set; } = false;
        public bool IsAttached => this.ffxivPlugin != null && IsDataSubscriptionHandled;

        public DFAEventSource(RainbowMage.OverlayPlugin.ILogger logger) : base(logger)
        {
            Name = "DFA";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            RegisterEventTypes(new List<string>()
            {
                "onAddonExampleOriginalTimerFiredEvent", "onDFAStatusUpdateEvent"
            });

            RegisterEventHandler("DFATTS", (msg) =>
            {
                string text = Config.TextToSpeech.Replace(@"${matched}", msg["text"].ToString());
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.TTS(text);
                return null;
            });
        }

        public override void Start()
        {
            onDFAStatusUpdate -= (e) => DispatchToJS(e);
            onDFAStatusUpdate += (e) => DispatchToJS(e);

            LogInfo(Config.StructuresURL);
            this.Config.Structures = UpdateStructuresAsync().Result;
            this.opcodeList = this.Config.Structures.Select(x => x.Opcode);

            this.ffxivPluginNetworkReceivedDelegate = new FFXIV_ACT_Plugin.Common.NetworkReceivedDelegate(NetworkReceived);
            this.ffxivPluginZoneChangedDelegate = new FFXIV_ACT_Plugin.Common.ZoneChangedDelegate(ZoneChanged);

            SetupTimers();
            StartTimers();
            LogInfo("DFA: Started.");
        }

        public override void Stop()
        {
            StopTimers();
            LogInfo("DFA: Stoped.");
        }

        protected override void Update()
        {
            // embedded timer is dsiabled
        }

        public override void Dispose()
        {
            DisposeTimers();
            base.Dispose();
        }


        public void SetupTimers()
        {
            processCheckTimer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = 1000,
            };
            processCheckTimer.Elapsed += (sender, e) =>
            {
                if (!IsAttached)
                {
                    Attach();
                }

                if (IsAttached)
                {
                    processCheckTimer.Interval = 10000;
                }
                else
                {
                    processCheckTimer.Interval = 1000;
                }
            };

            updateEventTimer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = 1000,
            };
            updateEventTimer.Elapsed += (sender, e) =>
            {
                if (IsAttached)
                {
                    onDFAStatusUpdate(new JSEvents.DFAStatusUpdateEvent(status.ToJson()));
                }
            };

        }

        public void StartTimers()
        {
            if (processCheckTimer != null)
            {
                processCheckTimer.Start();
            }

            if(updateEventTimer != null)
            {
                updateEventTimer.Start();
            }
        }

        public void StopTimers()
        {
            if (processCheckTimer != null)
            {
                processCheckTimer.Stop();
            }

            if (updateEventTimer != null)
            {
                updateEventTimer.Stop();
            }
        }

        public void DisposeTimers()
        {
            if (processCheckTimer != null)
            {
                processCheckTimer.Dispose();
            }

            if (updateEventTimer != null)
            {
                updateEventTimer.Dispose();
            }
        }




        public async Task<List<DFAEventSourceConfig.Structure>> UpdateStructuresAsync()
        {
            List<DFAEventSourceConfig.Structure> result = new List<DFAEventSourceConfig.Structure>();

            if (Config.StructuresURL == null || string.IsNullOrWhiteSpace(Config.StructuresURL))
            {
                return result;
            }

            if (!Uri.IsWellFormedUriString(Config.StructuresURL, UriKind.Absolute))
            {
                LogError("GetStructures: Invalid URL: {0}", Config.StructuresURL);
                return result;
            }

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await httpClient.GetAsync(Config.StructuresURL).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError("GetStructures: {0} Exception: {1}", Config.StructuresURL, ex.Message);
                return result;
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                LogError("GetStructures: {0} Response: {1}", Config.StructuresURL, httpResponse.StatusCode);
                return result;
            }

            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result = JsonConvert.DeserializeObject<List<DFAEventSourceConfig.Structure>>(content);
            }
            catch (Exception ex)
            {
                LogError("GetStructures: {0} Exception on Deserialize: {1}", Config.StructuresURL, ex.Message);
                return result;
            }

            return result;
        }


        public void Attach()
        {
            lock (this)
            {
                if (ActGlobals.oFormActMain == null)
                {
                    this.ffxivPlugin = null;
                    return;
                }

                if (this.ffxivPlugin == null)
                {
                    this.ffxivPlugin =
                         ActGlobals.oFormActMain.ActPlugins
                         .Where(x =>
                         x.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) &&
                         x.lblPluginStatus.Text.ToUpper().Contains("FFXIV Plugin Started.".ToUpper()))
                         .Select(x => x.pluginObj)
                         .FirstOrDefault();
                    return;
                }

                if (this.ffxivPlugin != null)
                {
                    try
                    {
                        ((FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)this.ffxivPlugin).DataSubscription.NetworkReceived -= ffxivPluginNetworkReceivedDelegate;
                        ((FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)this.ffxivPlugin).DataSubscription.NetworkReceived += ffxivPluginNetworkReceivedDelegate;
                        ((FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)this.ffxivPlugin).DataSubscription.ZoneChanged -= ffxivPluginZoneChangedDelegate;
                        ((FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)this.ffxivPlugin).DataSubscription.ZoneChanged += ffxivPluginZoneChangedDelegate;
                        IsDataSubscriptionHandled = true;
                        LogInfo("DataSubscription Handled");
                    }
                    catch
                    {
                        IsDataSubscriptionHandled = false;
                    }
                }
            }
        }


        public void NetworkReceived(string connection, long epoch, byte[] message)
        {
            HandleMessage(message);
        }

        public void ZoneChanged(uint zoneId, string zoneName)
        {
            if (status.MatchingState == MatchingState.MATCHED)
            {
                lock (status.LockObject)
                {
                    status.Clear();
                }

                onDFAStatusUpdate(new JSEvents.DFAStatusUpdateEvent(status.ToJson()));
                LogInfo("DFA: ZoneChanged: {0}, {1}", zoneId, zoneName);
            }
        }

        private void HandleMessage(byte[] message)
        {
            try
            {
                if (message.Length < 32)
                {
                    return;
                }

                var opcode = BitConverter.ToUInt16(message, 18);

                // 必要のないopcodeはここで弾く
                if (!opcodeList.Contains(opcode))
                {
                    return;
                }

                var data = message.Skip(32).ToArray();

                try
                {
                    if (opcode == Config.Structures.FirstOrDefault(x => x.Name == "DutyAccepted").Opcode)
                    {
                        var structure = Config.Structures.FirstOrDefault(x => x.Name == "DutyAccepted");
                        var roulette = BitConverter.ToUInt16(data, structure.Offset.RouletteCode);
                        var dungeon = BitConverter.ToUInt16(data, structure.Offset.DungeonCode);

                        lock (status.LockObject)
                        {
                            status.SetZero();
                            if (roulette != 0)
                            {
                                status.RouletteCode = roulette;
                            }
                            else
                            {
                                status.DungeonCode = dungeon;
                            }
                            status.MatchingState = MatchingState.QUEUED;
                        }

                        onDFAStatusUpdate(new JSEvents.DFAStatusUpdateEvent(status.ToJson()));
                        LogInfo("DFA: Queued [{0}/{1}]", roulette, dungeon);

                    }
                    else if (opcode == Config.Structures.FirstOrDefault(x => x.Name == "Matched").Opcode)
                    {
                        var structure = Config.Structures.FirstOrDefault(x => x.Name == "Matched");
                        var roulette = BitConverter.ToUInt16(data, structure.Offset.RouletteCode);
                        var dungeon = BitConverter.ToUInt16(data, structure.Offset.DungeonCode);

                        lock (status.LockObject)
                        {
                            status.SetZero();
                            status.RouletteCode = roulette;
                            status.DungeonCode = dungeon;
                            status.MatchingState = MatchingState.MATCHED;
                        }

                        /*
                        if (!string.IsNullOrWhiteSpace(TTS))
                        {
                            DFACoreLog("TTS RawString   : " + TTS);
                            var ttsString = ReplaceTtsVars(TTS, RouletteCode, Code);
                            DFACoreLog("TTS SpeachString: " + ttsString);
                            ActGlobals.oFormActMain.TTS(ttsString);
                        }
                        */

                        onDFAStatusUpdate(new JSEvents.DFAStatusUpdateEvent(status.ToJson()));
                        LogInfo("DFA: Matched [{0}/{1}]", roulette, dungeon);

                    }
                    else if (opcode == Config.Structures.FirstOrDefault(x => x.Name == "Canceled").Opcode)
                    {
                        var structure = Config.Structures.FirstOrDefault(x => x.Name == "Canceled");
                        var opcode2 = structure.Opcode2;

                        if (data[structure.Offset.Opcode2] == structure.Opcode2)
                        {
                            lock (status.LockObject)
                            {
                                status.Clear();
                            }
                        }

                        onDFAStatusUpdate(new JSEvents.DFAStatusUpdateEvent(status.ToJson()));
                        LogInfo("DFA: Canceled");
                    }
                    else if (opcode == Config.Structures.FirstOrDefault(x => x.Name == "WaitQueue").Opcode)
                    {
                        var structure = Config.Structures.FirstOrDefault(x => x.Name == "WaitQueue");
                        var waitList = data[structure.Offset.WaitList];
                        var waitTime = data[structure.Offset.WaitTime];
                        var tank = data[structure.Offset.Tank];
                        var tankMax = data[structure.Offset.TankMax];
                        var healer = data[structure.Offset.Healer];
                        var healerMax = data[structure.Offset.HealerMax];
                        var dps = data[structure.Offset.Dps];
                        var dpsMax = data[structure.Offset.DpsMax];

                        lock (status.LockObject)
                        {
                            status.WaitList = waitList;
                            status.WaitTime = waitTime;
                            status.Tank = tank;
                            status.TankMax = tankMax;
                            status.Healer = healer;
                            status.HealerMax = healerMax;
                            status.Dps = dps;
                            status.DpsMax = dpsMax;
                            status.MatchingState = MatchingState.QUEUED;
                        }

                        onDFAStatusUpdate(new JSEvents.DFAStatusUpdateEvent(status.ToJson()));
                        LogInfo("DFA: Queued: WaitList:{0} WaitTime:{1} Tank:{2}/{3} Healer:{4}/{5} DPS:{6}/{7}",
                            waitList, waitTime, tank, tankMax, healer, healerMax, dps, dpsMax);

                    }
                    else if (opcode == Config.Structures.FirstOrDefault(x => x.Name == "PartyUpdate").Opcode)
                    {
                        var structure = Config.Structures.FirstOrDefault(x => x.Name == "PartyUpdate");
                        var tank = data[structure.Offset.Tank];
                        var tankMax = data[structure.Offset.TankMax];
                        var healer = data[structure.Offset.Healer];
                        var healerMax = data[structure.Offset.HealerMax];
                        var dps = data[structure.Offset.Dps];
                        var dpsMax = data[structure.Offset.DpsMax];

                        lock (status.LockObject)
                        {
                            status.Tank = tank;
                            status.TankMax = tankMax;
                            status.Healer = healer;
                            status.HealerMax = healerMax;
                            status.Dps = dps;
                            status.DpsMax = dpsMax;
                            status.MatchingState = MatchingState.MATCHED;
                        }

                        onDFAStatusUpdate(new JSEvents.DFAStatusUpdateEvent(status.ToJson()));
                        LogInfo("DFA: Matched: Tank:{0}/{1} Healer:{2}/{3} DPS:{4}/{5}",
                            tank, tankMax, healer, healerMax, dps, dpsMax);

                    }
                    else if (opcode == Config.Structures.FirstOrDefault(x => x.Name == "Completed").Opcode)
                    {
                        lock (status.LockObject)
                        {
                            status.Clear();
                        }

                        onDFAStatusUpdate(new JSEvents.DFAStatusUpdateEvent(status.ToJson()));
                        LogInfo("DFA: Competed");
                    }
                }
                catch (Exception ex)
                {
                    LogInfo("DFA: Exception: Opcode:{0}, {1}", opcode, ex.Message);
                }
            }
            catch (Exception ex)
            {
                LogInfo("DFA: Exception: {0}", ex.Message);
            }
        }




        public override System.Windows.Forms.Control CreateConfigControl()
        {
            return new DFAEventSourceConfigPanel(this);
        }

        public override void LoadConfig(IPluginConfig config)
        {
            Config = DFAEventSourceConfig.LoadConfig(config);
        }

        public override void SaveConfig(IPluginConfig config)
        {
            Config.SaveConfig(config);
        }

        public void DispatchToJS(JSEvent e)
        {
            JObject ev = new JObject();
            ev["type"] = e.EventName();
            ev["detail"] = JObject.FromObject(e);
            DispatchEvent(ev);
        }

        private void DFACoreLog(string message, bool isDebugLog = false)
        {
            if (isDebugLog)
            {
                LogDebug(message);

            }
            else
            {
                LogInfo(message);
            }

        }
        public void LogDebug(string format, params object[] args)
        {
            this.Log(LogLevel.Debug, format, args);
        }

        public void LogError(string format, params object[] args)
        {
            this.Log(LogLevel.Error, format, args);
        }

        public void LogWarning(string format, params object[] args)
        {
            this.Log(LogLevel.Warning, format, args);
        }

        public void LogInfo(string format, params object[] args)
        {
            this.Log(LogLevel.Info, format, args);
        }
    }

    public interface JSEvent
    {
        string EventName();
    };

    public class JSEvents
    {
        public class DFAStatusUpdateEvent : JSEvent
        {
            public string statusJson;
            public DFAStatusUpdateEvent(string statusJson) { this.statusJson = statusJson; }
            public string EventName() { return "onDFAStatusUpdateEvent"; }

        }

    }

    public interface ILogger
    {
        void LogDebug(string format, params object[] args);
        void LogError(string format, params object[] args);
        void LogWarning(string format, params object[] args);
        void LogInfo(string format, params object[] args);
    }

}
