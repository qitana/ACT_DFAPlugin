/**
 * Analyze and HandleMessage Methods referred to easly1989/ffxiv_act_dfassist,
 * under the GNU General Public License v3.0.
 * 
 * https://github.com/easly1989/ffxiv_act_dfassist/blob/master/DFAssist.Core/Network/FFXIVPacketHandler.cs
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin;
using FFXIV_ACT_Plugin.Memory;
using FFXIV_ACT_Plugin.Network;
using Machina;
using Machina.FFXIV;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace Qitana.DFAPlugin
{
    public sealed class DFACore : IDisposable
    {
        private bool enableDebugLogging = false;

        private IActPluginV1 ffxivPlugin;
        private ProcessManager processManager;
        private TCPNetworkMonitor tcpNetworkMonitor;
        private FFXIVNetworkMonitor ffxivNetworkMonitor;

        private Dictionary<string, DFADataModel> dfaData = new Dictionary<string, DFADataModel>();
        private Dictionary<string, string> jsonFiles = new Dictionary<string, string>()
        {
            { "ja_jp", "ja-jp.json" },
            { "en_us", "en-us.json" },
            { "de_De", "de-de.json" },
            { "fr_fr", "fr-fr.json" },
            { "ko_kr", "ko-kr.json" },
        };

        private FFXIVNetworkMonitor.MessageReceivedDelegate messageReceivedDelegate;
        private TCPNetworkMonitor.DataReceivedDelegate dataRecievedDelegate; // 使わない

        private MatchingState _state = MatchingState.IDLE;
        public string TTS { get; set; } = string.Empty;

        private bool IsProcessChanged { get; set; } = false;
        public bool IsActive => true;
        public string State => this._state.ToString();
        public int RouletteCode { get; private set; } = 0;
        public DFADataModel.Names RouletteName => this.GetRouletteNames(this.RouletteCode);
        public int Code { get; private set; } = 0;
        public DFADataModel.Names Name => this.GetInstanceNames(this.Code);
        public uint WaitTime { get; private set; } = 0;
        public uint WaitList { get; private set; } = 0;

        public uint QueuedTank { get; private set; } = 0;
        public uint QueuedHealer { get; private set; } = 0;
        public uint QueuedDps { get; private set; } = 0;
        public uint QueuedTankMax { get; private set; } = 0;
        public uint QueuedHealerMax { get; private set; } = 0;
        public uint QueuedDpsMax { get; private set; } = 0;

        public uint MatchedTank { get; private set; } = 0;
        public uint MatchedHealer { get; private set; } = 0;
        public uint MatchedDps { get; private set; } = 0;
        public uint MatchedTankMax { get; private set; } = 0;
        public uint MatchedHealerMax { get; private set; } = 0;
        public uint MatchedDpsMax { get; private set; } = 0;


        public DFACore()
        {
            LoadJsonFiles();
            this.messageReceivedDelegate = new FFXIVNetworkMonitor.MessageReceivedDelegate(MessageReceived);
            this.dataRecievedDelegate = new TCPNetworkMonitor.DataReceivedDelegate(DataRecieved);　// 使わない
        }

        public void Dispose()
        {
            Stop();
        }

        public void LoadJsonFiles()
        {
            try
            {
                var dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var dfaDataDirectroy = Path.Combine(Directory.GetParent(Path.GetDirectoryName(dllPath)).FullName, "resources", "DFAPlugin", "data");
                if (Directory.Exists(dfaDataDirectroy))
                {
                    DFACoreLog("Load JSON Files From: " + dfaDataDirectroy);

                    foreach (var jsonFile in jsonFiles)
                    {
                        var jsonFilePath = Path.Combine(dfaDataDirectroy, jsonFile.Value);
                        if (File.Exists(jsonFilePath))
                        {
                            try
                            {
                                //dynamic data = JObject.Parse(File.ReadAllText(jsonFilePath));
                                DFADataModel data = JsonConvert.DeserializeObject<DFADataModel>(File.ReadAllText(jsonFilePath));
                                dfaData.Add(jsonFile.Key, data);
                            }
                            catch (Exception ex)
                            {
                                DFACoreLog("Failed to Load File: " + jsonFilePath + " : " + ex.Message.ToString());
                            }
                            finally
                            {
                                if (!dfaData.ContainsKey(jsonFile.Key))
                                {
                                    dfaData.Add(jsonFile.Key, null);
                                }
                            }
                        }
                        else
                        {
                            DFACoreLog("File Not Found: " + jsonFilePath);
                        }

                    }
                }
                else
                {
                    DFACoreLog("Directory Not Found: " + dfaDataDirectroy);
                }
            }
            catch (Exception ex)
            {
                DFACoreLog("Exception on Load(): " + ex.Message.ToString());
            }
        }

        public void Stop()
        {
            if (this.ffxivNetworkMonitor != null)
            {
                ffxivNetworkMonitor.MessageReceived -= this.messageReceivedDelegate;
            }

            if (this.tcpNetworkMonitor != null)
            {
                tcpNetworkMonitor.DataReceived -= this.dataRecievedDelegate;
            }
        }

        public bool IsAttached
            => this.ffxivPlugin != null && this.IsProcessChanged != true &&
            this.processManager != null && this.ffxivNetworkMonitor != null && this.tcpNetworkMonitor != null;

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
                    if (this.IsProcessChanged == true || this.ffxivNetworkMonitor == null || this.tcpNetworkMonitor == null)
                    {
                        this.ffxivNetworkMonitor = null;
                        this.tcpNetworkMonitor = null;
                        GC.Collect();

                        try
                        {
                            DFACoreLog("Attach Start.", true);

                            var dataCollectionFieldInfo =
                                ((FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)this.ffxivPlugin)
                                .GetType()
                                .GetField("_dataCollection", BindingFlags.NonPublic | BindingFlags.Instance);

                            if (dataCollectionFieldInfo != null)
                            {
                                var dataCollection = dataCollectionFieldInfo.GetValue(this.ffxivPlugin);

                                if (dataCollection != null)
                                {
                                    var processManagerFieldInfo =
                                        ((DataCollection)dataCollection)
                                        .GetType()
                                        .GetField("_processManager", BindingFlags.NonPublic | BindingFlags.Instance);

                                    if (processManagerFieldInfo != null)
                                    {
                                        var processManager = processManagerFieldInfo.GetValue(dataCollection);

                                        if (processManager != null)
                                        {
                                            this.processManager = (ProcessManager)processManager;
                                            this.processManager.ProcessChanged -= ProcessChanged;
                                            this.processManager.ProcessChanged += ProcessChanged;
                                        }
                                    }


                                    var scanPacketsFiledInfo =
                                        ((DataCollection)dataCollection)
                                        .GetType()
                                        .GetField("_scanPackets", BindingFlags.NonPublic | BindingFlags.Instance);

                                    if (scanPacketsFiledInfo != null)
                                    {
                                        var scanPackets = scanPacketsFiledInfo.GetValue(dataCollection);

                                        if (scanPackets != null)
                                        {
                                            var ffxivMonitorFiledInfo =
                                                ((ScanPackets)scanPackets)
                                                .GetType()
                                                .GetField("_monitor", BindingFlags.NonPublic | BindingFlags.Instance);

                                            if (ffxivMonitorFiledInfo != null)
                                            {
                                                var ffxivMonitor = ffxivMonitorFiledInfo.GetValue(scanPackets);
                                                if (ffxivMonitor != null)
                                                {
                                                    // Machina.FFXIV によるDecompress済のデータを使う場合はこのポイント
                                                    this.ffxivNetworkMonitor = (FFXIVNetworkMonitor)ffxivMonitor;
                                                    ffxivNetworkMonitor.MessageReceived -= this.messageReceivedDelegate;
                                                    ffxivNetworkMonitor.MessageReceived += this.messageReceivedDelegate;

                                                    var tcpMonitorFiledInfo =
                                                        ((FFXIVNetworkMonitor)ffxivMonitor)
                                                        .GetType()
                                                        .GetField("_monitor", BindingFlags.NonPublic | BindingFlags.Instance);
                                                    if (tcpMonitorFiledInfo != null)
                                                    {
                                                        var tcpMonitor = tcpMonitorFiledInfo.GetValue(ffxivMonitor);

                                                        if (tcpMonitor != null)
                                                        {
                                                            // Machina によるTCPのペイロードデータを使う場合はこのポイント
                                                            this.tcpNetworkMonitor = (TCPNetworkMonitor)tcpMonitor;
                                                            //tcpNetworkMonitor.DataReceived -= this.dataRecievedDelegate;
                                                            //tcpNetworkMonitor.DataReceived += this.dataRecievedDelegate;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (this.ffxivNetworkMonitor == null || this.tcpNetworkMonitor == null)
                            {
                                this.ffxivPlugin = null;
                                this.ffxivNetworkMonitor = null;
                                this.tcpNetworkMonitor = null;
                                DFACoreLog("Attach Failed.", true);
                            }
                            else
                            {
                                DFACoreLog("Attach Success.");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.ffxivPlugin = null;
                            this.ffxivNetworkMonitor = null;
                            this.tcpNetworkMonitor = null;
                            DFACoreLog(ex.ToString());
                        }
                        finally
                        {
                            this.IsProcessChanged = false;
                        }
                    }
                }
            }
        }

        private void ProcessChanged(object sender, EventArgs e)
        {
            DFACoreLog("ProcessChanged.");
            this.IsProcessChanged = true;
        }

        public void MessageReceived(string connection, long epoch, byte[] data)
        {
            HandleMessage(data, ref this._state);
        }


        public void DataRecieved(string connection, byte[] data)
        {
            Analyze(data, ref this._state);
        }

        public void Analyze(byte[] payload, ref MatchingState state)
        {
            try
            {
                while (true)
                {
                    if (payload.Length < 4)
                        break;

                    var type = BitConverter.ToUInt16(payload, 0);
                    if (type == 0x0000 || type == 0x5252)
                    {
                        if (payload.Length < 28)
                            break;

                        var length = BitConverter.ToInt32(payload, 24);
                        if (length <= 0 || payload.Length < length)
                            break;

                        using (var messages = new MemoryStream(payload.Length))
                        {
                            using (var stream = new MemoryStream(payload, 0, length))
                            {
                                stream.Seek(40, SeekOrigin.Begin);

                                if (payload[33] == 0x00)
                                {
                                    stream.CopyTo(messages);
                                }
                                else
                                {
                                    // .Net DeflateStream Bug (Force the previous 2 bytes)
                                    stream.Seek(2, SeekOrigin.Current);
                                    using (var z = new DeflateStream(stream, CompressionMode.Decompress))
                                    {
                                        z.CopyTo(messages);
                                    }
                                }
                            }

                            messages.Seek(0, SeekOrigin.Begin);

                            var messageCount = BitConverter.ToUInt16(payload, 30);
                            for (var i = 0; i < messageCount; i++)
                            {
                                try
                                {
                                    var buffer = new byte[4];
                                    var read = messages.Read(buffer, 0, 4);
                                    if (read < 4)
                                    {
                                        DFACoreLog($"A: Length Error while analyzing Message: {read} {i}/{messageCount}");
                                        break;
                                    }

                                    var messageLength = BitConverter.ToInt32(buffer, 0);

                                    var message = new byte[messageLength];
                                    messages.Seek(-4, SeekOrigin.Current);
                                    messages.Read(message, 0, messageLength);

                                    HandleMessage(message, ref state);
                                }
                                catch (Exception ex)
                                {
                                    DFACoreLog($"A: Error while analyzing Message: {ex.ToString()}");
                                }
                            }
                        }

                        if (length < payload.Length)
                        {
                            // Packets still need to be processed
                            payload = payload.Skip(length).ToArray();
                            continue;
                        }
                    }
                    else
                    {
                        // Forward-Cut packet workaround
                        // Discard one truncated packet and find just the next packet
                        for (var offset = 0; offset < payload.Length - 2; offset++)
                        {
                            var possibleType = BitConverter.ToUInt16(payload, offset);
                            if (possibleType != 0x5252)
                                continue;

                            payload = payload.Skip(offset).ToArray();
                            Analyze(payload, ref state);
                            break;
                        }
                    }

                    break;
                }
            }
            catch (Exception ex)
            {
                DFACoreLog($"A: Error while handling Message: {ex.ToString()}");
            }
        }

        private void HandleMessage(byte[] message, ref MatchingState state)
        {
            try
            {
                if (message.Length < 32)
                {
                    // type == 0x0000 (Messages were filtered here)
                    return;
                }

                var opcode = BitConverter.ToUInt16(message, 18);

#if !DEBUG
                // 本番用。使うものだけ入れる
                if (
                    opcode != 0x0164 &&
                    opcode != 0x032D &&
                    opcode != 0x03CF &&
                    opcode != 0x02A8 &&
                    opcode != 0x032F &&
                    opcode != 0x0339 &&
                    opcode != 0x0002
                    )
                    return;
#endif

#if DEBUG
                // opcodeが変わったと思われる場合はここを全部作り直し
                // ハウス等で何もしなくても出るものをブロック。平時でも出るものは使えない。
                if (
                    opcode == 0x0000 ||
                    opcode == 0x012D ||
                    opcode == 0x00EB ||
                    opcode == 0x026A ||
                    opcode == 0x03C5 ||
                    opcode == 0x0065 ||
                    opcode == 0x031D ||
                    opcode == 0x0159 ||
                    opcode == 0x0000 ||
                    opcode == 0x0000 ||
                    opcode == 0x0000 ||
                    opcode == 0x0000 ||
                    opcode == 0x0000
                    )
                    return;

                // opcodeが変わったと思われる場合はここを全部作り直し
                // CF関連で出るものを列挙。ここに入れればログにByte列が出力される。
                if (
                    opcode == 0x0000 ||
                    opcode == 0x0164 ||
                    opcode == 0x03CF ||
                    opcode == 0x02A8 ||
                    opcode == 0x017C ||
                    opcode == 0x008D ||
                    opcode == 0x02D6 ||
                    opcode == 0x033C ||
                    opcode == 0x03CF ||
                    opcode == 0x02A3 ||
                    opcode == 0x032D ||
                    opcode == 0x0347 ||
                    opcode == 0x032F ||
                    opcode == 0x00D7 ||
                    opcode == 0x0339 ||
                    opcode == 0x1008 ||
                    opcode == 0x0002 ||
                    opcode == 0x0198 ||
                    opcode == 0x0000 ||
                    opcode == 0x0000
                    )

                {
                    var d = message.Skip(32).Take(32).ToArray();
                    DFACoreLog($"Opcode: [{opcode.ToString("X4")}] {BitConverter.ToString(d)}");
                }
                else
                {
                    // opcodeが変わったと思われる場合、Opcodeを出力して全部確認する
                    // 以下のコメントアウトを外して確認する。
                    //DFACoreLog($"Opcode: [{opcode.ToString("X4")}]");
                    return;
                }

#endif
                var data = message.Skip(32).ToArray();
                switch (opcode)
                {
                    case 0x0164: // Duty 5.11
                        var duty_roulette = BitConverter.ToUInt16(data, 8);
                        var duty_code = BitConverter.ToUInt16(data, 12);

                        RouletteCode = Code = 0;
                        QueuedTank = QueuedHealer = QueuedDps = QueuedTankMax = QueuedHealerMax = QueuedDpsMax = 0;
                        MatchedTank = MatchedHealer = MatchedDps = MatchedTankMax = MatchedHealerMax = MatchedDpsMax = 0;
                        WaitTime = WaitList = 0;

                        if (duty_roulette != 0)
                        {
                            RouletteCode = duty_roulette;
                            Code = 0;
                            state = MatchingState.QUEUED;
                        }
                        else
                        {
                            RouletteCode = 0;
                            Code = duty_code;
                            state = MatchingState.QUEUED;
                        }
                        DFACoreLog($"Q: QUEUED [{duty_roulette}/{duty_code}]");
                        break;

                    case 0x032D: // Matched 5.11
                        var matched_roulette = BitConverter.ToUInt16(data, 2);
                        var matched_code = BitConverter.ToUInt16(data, 20);
                        RouletteCode = matched_roulette;
                        Code = matched_code;
                        state = MatchingState.MATCHED;
                        if (!string.IsNullOrWhiteSpace(TTS))
                        {
                            DFACoreLog("TTS RawString   : " + TTS);
                            var ttsString = ReplaceTtsVars(TTS, RouletteCode, Code);
                            DFACoreLog("TTS SpeachString: " + ttsString);
                            ActGlobals.oFormActMain.TTS(ttsString);
                        }
                        DFACoreLog($"Q: Matched [{matched_roulette}/{matched_code}]");
                        break;

                    case 0x03CF: // operation? 5.11
                        switch (data[0])
                        {
                            case 0x73: // canceled (by me?)
                                RouletteCode = Code = 0;
                                QueuedTank = QueuedHealer = QueuedDps = QueuedTankMax = QueuedHealerMax = QueuedDpsMax = 0;
                                MatchedTank = MatchedHealer = MatchedDps = MatchedTankMax = MatchedHealerMax = MatchedDpsMax = 0;
                                WaitTime = WaitList = 0;
                                state = MatchingState.IDLE;
                                DFACoreLog($"Q: Canceled");
                                break;
                            case 0x81: // duty requested
                                break;
                            default:
                                break;
                        }
                        break;

                    case 0x02A8: // wait queue update 5.11
                        var waitList = data[6];
                        var waitTime = data[7];
                        var queuedTank = data[8];
                        var queuedTankMax = data[9];
                        var queuedHealer = data[10];
                        var queuedHealerMax = data[11];
                        var queuedDps = data[12];
                        var queuedDpsMax = data[13];

                        if (state == MatchingState.MATCHED)
                        {
                            state = MatchingState.QUEUED;
                            MatchedTank = MatchedHealer = MatchedDps = MatchedTankMax = MatchedHealerMax = MatchedDpsMax = 0;
                        }

                        WaitList = waitList;
                        WaitTime = waitTime;
                        QueuedTank = queuedTank;
                        QueuedHealer = queuedHealer;
                        QueuedDps = queuedDps;
                        QueuedTankMax = queuedTankMax;
                        QueuedHealerMax = queuedHealerMax;
                        QueuedDpsMax = queuedDpsMax;

                        DFACoreLog($"Q: waitList:{waitList} waitTime:{waitTime} tank:{queuedTank}/{queuedTankMax} healer:{queuedHealer}/{queuedHealerMax} dps:{queuedDps}/{queuedDpsMax}");
                        break;

                    case 0x032F: // party update after matched 5.11
                        var matchedTank = data[12];
                        var matchedTankMax = data[13];
                        var matchedHealer = data[14];
                        var matchedHealerMax = data[15];
                        var matchedDps = data[16];
                        var matchedDpsMax = data[17];

                        MatchedTank = matchedTank;
                        MatchedHealer = matchedHealer;
                        MatchedDpsMax = matchedDpsMax;
                        MatchedTankMax = matchedTankMax;
                        MatchedHealerMax = matchedHealerMax;
                        MatchedDps = matchedDps;

                        DFACoreLog($"M: tank:{matchedTank}/{matchedTankMax} healer:{matchedHealer}/{matchedHealerMax} dps:{matchedDps}/{matchedDpsMax}");
                        break;

                    case 0x0339: // area change 5.11
                        var area_code = BitConverter.ToUInt16(data, 4);

                        if (state == MatchingState.MATCHED)
                        {
                            state = MatchingState.IDLE;
                            DFACoreLog($"I: Entered Area [{area_code}]");
                        }
#if DEBUG
                        DFACoreLog($"Entered Area [{area_code}]");
#endif
                        break;

                    case 0x0002: // matching complete flag?
                        if (state == MatchingState.MATCHED)
                        {
                            state = MatchingState.IDLE;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DFACoreLog($"A: Error while analyzing Message: {ex.ToString()}");
            }
        }
        public enum MatchingState : int
        {
            IDLE = 0,
            QUEUED = 1,
            MATCHED = 2,
        }

        private string ReplaceTtsVars(string text, int rouletteCode, int instanceCode)
        {

            try
            {
                if (text.Contains(@"{roulette.ja_jp}")) text = text.Replace(@"{roulette.ja_jp}", GetRouletteName("ja_jp", rouletteCode));
                if (text.Contains(@"{roulette.en_us}")) text = text.Replace(@"{roulette.en_us}", GetRouletteName("en_us", rouletteCode));
                if (text.Contains(@"{roulette.de_de}")) text = text.Replace(@"{roulette.de_de}", GetRouletteName("de_de", rouletteCode));
                if (text.Contains(@"{roulette.fr_fr}")) text = text.Replace(@"{roulette.fr_fr}", GetRouletteName("fr_fr", rouletteCode));
                if (text.Contains(@"{roulette.ko_kr}")) text = text.Replace(@"{roulette.ko_kr}", GetRouletteName("ko_kr", rouletteCode));

                if (text.Contains(@"{instance.ja_jp}")) text = text.Replace(@"{instance.ja_jp}", GetInstanceName("ja_jp", instanceCode));
                if (text.Contains(@"{instance.en_us}")) text = text.Replace(@"{instance.en_us}", GetInstanceName("en_us", instanceCode));
                if (text.Contains(@"{instance.de_de}")) text = text.Replace(@"{instance.de_de}", GetInstanceName("de_de", instanceCode));
                if (text.Contains(@"{instance.fr_fr}")) text = text.Replace(@"{instance.fr_fr}", GetInstanceName("fr_fr", instanceCode));
                if (text.Contains(@"{instance.ko_kr}")) text = text.Replace(@"{instance.ko_kr}", GetInstanceName("ko_kr", instanceCode));
            }
            catch (Exception ex)
            {
                DFACoreLog("ReplaceTtsVars Error: " + ex.Message);
            }

            return text;
        }

        public DFADataModel.Names GetRouletteNames(int rouletteCode)
        {
            if(rouletteCode == 0)
            {
                return new DFADataModel.Names()
                {
                    ja_jp = string.Empty,
                    en_us = string.Empty,
                    de_de = string.Empty,
                    fr_fr = string.Empty,
                    ko_kr = string.Empty,
                };
            }

            return new DFADataModel.Names()
            {
                ja_jp = GetRouletteName("ja_jp", rouletteCode),
                en_us = GetRouletteName("en_us", rouletteCode),
                de_de = GetRouletteName("de_de", rouletteCode),
                fr_fr = GetRouletteName("fr_fr", rouletteCode),
                ko_kr = GetRouletteName("ko_kr", rouletteCode),
            };
        }

        private string GetRouletteName(string locale, int rouletteCode)
        {
            if (!this.dfaData.ContainsKey(locale))
            {
                return "Unknown Roulette";
            }

            if (this.dfaData[locale].roulettes == null ||
                !this.dfaData[locale].roulettes.ContainsKey(rouletteCode.ToString()))
            {
                return "Unknown Roulette";
            }

            return this.dfaData[locale].roulettes[rouletteCode.ToString()];
        }

        public DFADataModel.Names GetInstanceNames(int instanceCode)
        {
            if (instanceCode == 0)
            {
                return new DFADataModel.Names()
                {
                    ja_jp = string.Empty,
                    en_us = string.Empty,
                    de_de = string.Empty,
                    fr_fr = string.Empty,
                    ko_kr = string.Empty,
                };
            }

            return new DFADataModel.Names()
            {
                ja_jp = GetInstanceName("ja_jp", instanceCode),
                en_us = GetInstanceName("en_us", instanceCode),
                de_de = GetInstanceName("de_de", instanceCode),
                fr_fr = GetInstanceName("fr_fr", instanceCode),
                ko_kr = GetInstanceName("ko_kr", instanceCode),
            };
        }

        private string GetInstanceName(string locale, int instanceCode)
        {
            if (!this.dfaData.ContainsKey(locale))
            { 
                return "Unknown Locale"; 
            }

            if (this.dfaData[locale].instances == null || 
                !this.dfaData[locale].instances.ContainsKey(instanceCode.ToString()))
            {
                return "Unknown Instance (" + instanceCode.ToString() + ")";
            }

            if(this.dfaData[locale].instances[instanceCode.ToString()] == null)
            {
                return "Unknown Instance (" + instanceCode.ToString() + ")";
            }

            return this.dfaData[locale].instances[instanceCode.ToString()].name;
        }

        #region Log
        static ReaderWriterLock rwl = new ReaderWriterLock();
        private void DFACoreLog(string message, bool isDebugLog = false)
        {
            if (isDebugLog == true && this.enableDebugLogging == false)
            {
                return;
            }

            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            try
            {
                rwl.AcquireWriterLock(1000);
                try
                {
                    System.IO.File.AppendAllText(
                        System.IO.Path.Combine(Path.GetTempPath(), "DFAPlugin.log"),
                        "[" + date + "] " + message + Environment.NewLine);
                }
                finally
                {
                    rwl.ReleaseWriterLock();
                }
            }
            catch
            { }
        }

        #endregion
    }
}
