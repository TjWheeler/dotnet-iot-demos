using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotEasyComponents.Basic
{
    /// <summary>
    /// Registers an event handler to count number of events occured.
    /// Event count is reset when requested via the GetEventCountAndReset() method.
    /// </summary>
    public class PinObserver : IDisposable
    {
        public PinObserver(GpioController controller, int pin, PinEventTypes watchEvent)
        {
            Controller = controller;
            Pin = pin;
            WatchEvent = watchEvent;
        }

        public GpioController Controller { get; }
        public int Pin { get; }
        public PinEventTypes WatchEvent { get; }
        private long _eventCount = 0;
        public void On()
        {
            ResetEventCount();
            if (!Controller.IsPinOpen(Pin))
            {
                Controller.OpenPin(Pin, PinMode.InputPullUp);
            }
            Controller.RegisterCallbackForPinValueChangedEvent(Pin, WatchEvent, OnPinChanged);
        }
        public long GetEventCountAndReset()
        {
            var numberOfEvents = _eventCount;
            ResetEventCount();
            return numberOfEvents;
        }
        public long PeekEventCount()
        {
            return _eventCount;
        }

        public void Off()
        {
            Controller.UnregisterCallbackForPinValueChangedEvent(Pin, OnPinChanged);
            if (Controller.IsPinOpen(Pin))
            {
                Controller.ClosePin(Pin);
            }
        }
        private void OnPinChanged(object sender, PinValueChangedEventArgs args)
        {
            _eventCount++;
        }
        private void ResetEventCount()
        {
            _eventCount = 0;
        }

        public void Dispose()
        {
            try
            {
                Off();
                ResetEventCount();
            }
            catch {    /* ignore */    }
        }
    }
}
