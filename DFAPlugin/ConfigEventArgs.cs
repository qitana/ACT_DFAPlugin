using System;

namespace Qitana.DFAPlugin
{
    public class IntervalChangedEventArgs : EventArgs
    {
        public int NewInterval { get; private set; }
        public IntervalChangedEventArgs(int newInterval)
        {
            this.NewInterval = newInterval;
        }
    }
}
