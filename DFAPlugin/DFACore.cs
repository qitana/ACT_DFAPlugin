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

namespace Qitana.DFAPlugin
{
    public sealed class DFACore : IDisposable
    {
        private bool enableDebugLogging = false;

        private IActPluginV1 ffxivPlugin;
        private ProcessManager processManager;
        private TCPNetworkMonitor tcpNetworkMonitor;
        private FFXIVNetworkMonitor ffxivNetworkMonitor;

        private FFXIVNetworkMonitor.MessageReceivedDelegate messageReceivedDelegate;
        private TCPNetworkMonitor.DataReceivedDelegate dataRecievedDelegate; // 使わない

        private MatchingState _state = MatchingState.IDLE;

        public string TTS { get; set; } = string.Empty;

        private bool IsProcessChanged { get; set; } = false;
        public bool IsActive => true;
        public string State => this._state.ToString();
        public int RouletteCode { get; private set; } = 0;
        public int Code { get; private set; } = 0;
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
            this.messageReceivedDelegate = new FFXIVNetworkMonitor.MessageReceivedDelegate(MessageReceived);
            this.dataRecievedDelegate = new TCPNetworkMonitor.DataReceivedDelegate(DataRecieved);　// 使わない
            Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            DFACoreLog(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
                                                            //Machina によるTCPのペイロードデータを使う場合はこのポイント
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
                    opcode != 0x008F &&
                    opcode != 0x00B3 &&
                    opcode != 0x009A &&
                    opcode != 0x0304 &&
                    opcode != 0x00AE &&
                    opcode != 0x0257
                    )
                    return;
#endif

#if DEBUG
                // opcodeが変わったと思われる場合はここを全部作り直し
                // 宿屋で何もしなくても出るものをブロック。
                if (
                    opcode == 0x022F ||
                    opcode == 0x0264 ||
                    opcode == 0x0346 ||
                    opcode == 0x00C7 ||
                    opcode == 0x0000 ||
                    opcode == 0x0000 ||
                    opcode == 0x0000
                    )
                    return;

                // opcodeが変わったと思われる場合はここを全部作り直し
                // CF関連で出るものを列挙。ここに入れればログにByte列が出力される。
                if (
                    opcode == 0x035A ||
                    opcode == 0x008F ||
                    opcode == 0x009A ||
                    opcode == 0x00B3 ||
                    opcode == 0x01C7 ||
                    opcode == 0x00AE ||
                    opcode == 0x0257 ||
                    opcode == 0x1019 ||
                    opcode == 0x0002 ||
                    opcode == 0x03D2 ||
                    opcode == 0x0304 ||
                    opcode == 0x02D6 ||
                    opcode == 0x0000 ||
                    opcode == 0x0000 ||
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
                    //DFACoreLog($"Opcode: [{opcode.ToString("X4")}]");
                    return;
                }

#endif
                var data = message.Skip(32).ToArray();
                switch (opcode)
                {
                    case 0x008F: // Duty
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

                    case 0x00B3: // Matched
                        var matched_roulette = BitConverter.ToUInt16(data, 2);
                        var matched_code = BitConverter.ToUInt16(data, 20);
                        RouletteCode = matched_roulette;
                        Code = matched_code;
                        state = MatchingState.MATCHED;
                        if (!string.IsNullOrWhiteSpace(TTS))
                        {
                            ActGlobals.oFormActMain.TTS(TTS);
                        }
                        DFACoreLog($"Q: Matched [{matched_roulette}/{matched_code}]");
                        break;

                    case 0x009A: // operation??
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

                    case 0x0304: // wait queue update
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

                    case 0x00AE: // party update after matched
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

                    case 0x0257: // area change
                        if (state == MatchingState.MATCHED)
                        {
                            state = MatchingState.IDLE;
                            var area_code = BitConverter.ToUInt16(data, 4);
                            DFACoreLog($"I: Entered Area [{area_code}]");
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
