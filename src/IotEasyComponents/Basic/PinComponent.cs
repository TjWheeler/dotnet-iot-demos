using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IotEasyComponents.Basic
{
    /// <summary>
    ///     Handles basic functions over normal GPIO Pin.
    /// </summary>
    /// <remarks>
    ///     This is not for PWM Pins.
    /// </remarks>
    public class PinComponent : IDisposable
    {
        public PinComponent(GpioController controller, int pin, PinMode pinMode)
        {
            Controller = controller;
            Pin = pin;
            PinMode = pinMode;
        }

        public GpioController Controller { get; }
        public int Pin { get; }
        public PinMode PinMode { get; }


        public async Task FlashAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                OnHigh();
                await Task.Delay(delay, cancellationToken);
                Off();
            }
        }
        public async Task FlashAsync(List<TimeSpan> times, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var queue = new Queue<TimeSpan>(times);
                while (queue.Count > 0)
                {
                    OnHigh();
                    await Task.Delay(queue.Dequeue(), cancellationToken);
                    Off();
                    await Task.Delay(queue.Dequeue(), cancellationToken);
                }
            }
            Trace.WriteLine($"Pin {Pin} flash sequence cancelled");
        }
        public async Task FlashAsync(TimeSpan onTime, TimeSpan offTime, CancellationToken cancellationToken)
        {
            Trace.WriteLine($"Setting Pin {Pin} to a flash sequence with an onTime of {onTime} and offTime of {offTime}");
            while (!cancellationToken.IsCancellationRequested)
            {
                OnHigh();
                await Task.Delay(onTime, cancellationToken);
                Off();
                await Task.Delay(offTime, cancellationToken);
            }

            Trace.WriteLine($"Pin {Pin} flash sequence cancelled");
        }

        public void OnHigh()
        {
            On();
            Controller.Write(Pin, PinValue.High);
        }

        public void OnLow()
        {
            On();
            Controller.Write(Pin, PinValue.Low);
        }

        public void On()
        {
            if (!Controller.IsPinOpen(Pin))
            {
                Controller.OpenPin(Pin, PinMode);
            }
        }

        public void Off()
        {
            if (Controller.IsPinOpen(Pin))
            {
                Controller.ClosePin(Pin);
            }
        }

        public void Dispose()
        {
            try
            {
                Off();
            }
            catch
            {
                // Ignore
            }
        }
    }
}