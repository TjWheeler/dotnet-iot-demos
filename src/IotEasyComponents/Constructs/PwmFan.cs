using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Threading;
using System.Threading.Tasks;
using IotEasyComponents.Basic;

namespace IotEasyComponents.Constructs
{
    /// <summary>
    /// This class manages a Pwm Fan, on a Pwm Chip.
    /// It also optionally samples the Tachometer to determine RPM by sampling over a 1 second timespan.
    /// </summary>
    /// <remarks>
    /// Ensure the PWM pin is enabled on your device.  The Tachometer connection should be plugged into a normal
    /// GPIO pin.  The Fan Pwm pin must be connected to a PWM pin on the device.
    /// </remarks>
    public class PwmFan : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;
        private DateTime _lastSample;
        private readonly PwmComponent _pwmPin;
        private Task _sampleTask;
        private readonly PinObserver _tachoPin;
        private const int TachoSampleMilliseconds = 1000;

        public PwmFan(GpioController gpioController, PwmController pwmController, int pwmChip,
            int pwmChannel, int hertz, int? tachoPin, int initialDutyCycle = 100)
        {
            _pwmPin = new PwmComponent(pwmController, pwmChip, pwmChannel, hertz, initialDutyCycle);
            if (tachoPin.HasValue)
            {
                _tachoPin = new PinObserver(gpioController, tachoPin.Value, PinEventTypes.Rising);
            }
        }

        public int RPM { get; private set; }

        public void On()
        {
            if (!_pwmPin.IsOn)
            {
                _pwmPin.On();
                if (_tachoPin != null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _sampleTask = Task.Factory.StartNew(WatchFanSpeedAsync, _cancellationTokenSource.Token);
                    _tachoPin.On();
                }
            }
        }

        public void Off()
        {
            if (_pwmPin.IsOn)
            {
                _pwmPin.Off();
                if (_tachoPin != null)
                {
                    _tachoPin.Off();
                    _cancellationTokenSource.CancelAfter(1000);
                }
            }
        }

        public void SetSpeed(int dutyCycle)
        {
            _pwmPin.ChangeDutyCycle(dutyCycle);
        }

        private async Task WatchFanSpeedAsync()
        {
            try
            {
                while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    var pulses = _tachoPin.GetEventCountAndReset();
                    var milliSeconds = (DateTime.Now - _lastSample).TotalMilliseconds;
                    var revsPerSecond = (pulses / 2) / (milliSeconds / TachoSampleMilliseconds);
                    RPM = Convert.ToInt32(revsPerSecond * 60);
                    _lastSample = DateTime.Now;
                    await Task.Delay(TachoSampleMilliseconds, _cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task cancel exception");
            }
        }

        public void Dispose()
        {
            try
            {
                Off();
                _pwmPin.Dispose();
                _tachoPin?.Dispose();
                if (_sampleTask != null)
                {
                    _sampleTask.Dispose();
                    _sampleTask = null;
                }
            }
            catch
            {
                // Ignore
                
            }
        }
    }
}