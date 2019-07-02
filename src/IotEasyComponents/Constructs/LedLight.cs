using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using IotEasyComponents.Basic;

namespace IotEasyComponents.Constructs
{
    /// <summary>
    /// Control an LED light connected to a GPIO pin.
    /// </summary>
    public class LedLight : PinComponent
    {
        public LedLight(GpioController controller, int pin) : base(controller, pin, PinMode.Output)
        {
        }

        public Task WarningFlash(CancellationToken cancellationToken)
        {
            return FlashAsync(new TimeSpan(0, 0, 0, 0, 500), cancellationToken);
        }
        /// <summary>
        /// Flash 3 times at .5 second intervals, then wait for 2 seconds.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ErrorFlash(CancellationToken cancellationToken)
        {
            var times = new List<TimeSpan>
            {
                new TimeSpan(0, 0, 0, 0, 500),
                new TimeSpan(0, 0, 0, 0, 500),
                new TimeSpan(0, 0, 0, 0, 500),
                new TimeSpan(0, 0, 0, 0, 500),
                new TimeSpan(0, 0, 0, 0, 500),
                new TimeSpan(0, 0, 0, 0, 500),
                new TimeSpan(0, 0, 0, 0, 2000)
            };
            return FlashAsync(times, cancellationToken);
        }
    }
}