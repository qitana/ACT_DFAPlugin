using System;
using System.Xml.Serialization;
using RainbowMage.OverlayPlugin;

namespace Qitana.DFAPlugin
{
    [Serializable]
    public class DFAOverlayConfig : OverlayConfigBase
    {
        public event EventHandler<IntervalChangedEventArgs> IntervalChanged;

        public DFAOverlayConfig(string name)
            : base(name)
        {
            this._Interval = 1000;
            this.Url = new Uri(System.IO.Path.Combine(OverlayAddonMain.ResourcesDirectory, @"DFAPlugin\DFAStatus.html")).ToString();
            this._TTS = string.Empty;
        }

        private DFAOverlayConfig()
            : base(null)
        {
        }

        public override Type OverlayType => typeof(DFAOverlay);

        private int _Interval;
        [XmlElement("Interval")]
        public int Interval
        {
            get
            {
                return this._Interval;
            }
            set
            {
                if (this._Interval != value)
                {
                    this._Interval = value;
                    IntervalChanged?.Invoke(this, new IntervalChangedEventArgs(this._Interval));
                }
            }
        }

        private string _TTS;
        [XmlElement("TTS")]
        public string TTS
        {
            get
            {
                return this._TTS;
            }
            set
            {
                if (this._TTS != value)
                {
                    this._TTS = value;
                }
            }
        }
    }
}
