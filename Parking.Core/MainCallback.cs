using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;

namespace Parking.Core
{
    public delegate void LocationWatchEventHandler(Location loc);
    public delegate void DeviceWatchEventHandler(Device dev);
    public delegate void FaultWatchEventHandler(Alarm fault);
    public class MainCallback
    {
        public event LocationWatchEventHandler LctnWatchEvent;
        public event DeviceWatchEventHandler DeviceWatchEvent;
        public event FaultWatchEventHandler FaultWatchEvent;

        private static readonly MainCallback _mainCallback = new MainCallback();
        private MainCallback()
        {
        }
        public static MainCallback FileWatch
        {
            get
            {
                return _mainCallback;
            }
        }

        public void OnLocationChange(Location loc)
        {
            if (LctnWatchEvent != null)
            {
                LctnWatchEvent(loc);
            }
        }

        public void OnDeviceChange(Device dev)
        {
            if (DeviceWatchEvent != null)
            {
                DeviceWatchEvent(dev);
            }
        }

        public void OnFaultChange(Alarm fault)
        {
            if (FaultWatchEvent != null)
            {
                FaultWatchEvent(fault);
            }
        }

    }
}
