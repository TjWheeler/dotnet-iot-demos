using System;
using System.Device.Pwm;

namespace IotEasyComponents.Basic
{
    /// <summary>
    ///     Basic methods over the PWM Pins.
    /// </summary>
    /// <remarks>
    ///     You may need to enable the PWM pin on your device. Duty cycle is in the range of 0-100.
    /// </remarks>
    public class PwmComponent : IDisposable
    {
        private int _dutyCycle;
        private int _hertz;

        public PwmComponent(PwmController controller, int pwmChip, int pwmChannel,
            int hertz, int dutyCycle = 100)
        {
            Controller = controller;
            PwmChip = pwmChip;
            PwmChannel = pwmChannel;
            Hertz = hertz;
            _dutyCycle = dutyCycle;
        }

        public int DutyCycle
        {
            set { ChangeDutyCycle(value); }
            get { return _dutyCycle; }
        }

        public PwmController Controller { get; }
        public int PwmChip { get; }
        public int PwmChannel { get; }

        public int Hertz
        {
            get { return _hertz; }
            set
            {
                _hertz = value;
                if (IsOn)
                {
                    On();
                }
            }
        }

        public bool IsOn { get; private set; }
        public bool IsOpen { get; private set; }

        public void ChangeDutyCycle(int dutyCycle)
        {
            if (dutyCycle < 0 || dutyCycle > 100)
            {
                throw new InvalidOperationException("You must specify a duty cycle in the 0-100 range");
            }
            Controller.ChangeDutyCycle(PwmChip, PwmChannel, _dutyCycle);
            _dutyCycle = dutyCycle;
        }

        public void Open()
        {
            Controller.OpenChannel(PwmChip, PwmChannel);
            IsOpen = true;
        }

        public void Close()
        {
            Controller.CloseChannel(PwmChip, PwmChannel);
            IsOpen = false;
        }

        public void On()
        {
            if (!IsOpen)
            {
                Open();
            }

            Controller.StartWriting(PwmChip, PwmChannel, Hertz, _dutyCycle);
            IsOn = true;
        }

        public void On(int dutyCycle)
        {
            if (!IsOpen)
            {
                Open();
            }

            _dutyCycle = dutyCycle;
            Controller.StartWriting(PwmChip, PwmChannel, Hertz, _dutyCycle);
            IsOn = true;
        }

        public void Off()
        {
            Controller.StopWriting(PwmChip, PwmChannel);
            IsOn = false;
        }
        public void Dispose()
        {
            if (IsOn)
            {
                try
                {
                    Off();
                    Close();
                }
                catch
                {
                }
            }
        }
    }
}