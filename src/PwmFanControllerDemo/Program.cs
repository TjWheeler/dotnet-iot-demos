﻿using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Threading.Tasks;
using IotEasyComponents.Constructs;

namespace PwmFanControllerDemo
{
    class Program
    {
        /// <summary>
        /// Starts a fan at 100%, then reduces it over time and outputs the RPM.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //PWMFanRealWorldExample();
            PWMFanSimpleExample();
        }

        private static void PWMFanRealWorldExample()
        {
            Console.WriteLine("Starting PWM Controller - Realworld Demo");
            var pwmController = new PwmController();
            var gpioController = new GpioController();
            try
            {
                var chip = 0; //On the Rapsberry Pi this will be GPIO 18
                var channel = 0; //On the Rapsberry Pi this will be GPIO 18
                var hertz = 25; //12v PWM PC Fans run on a 25 hertz frequency
                int? tachoPin = 23; //A normal GPIO pin connected to the tachometer pin on the fan
                using (var fan = new PwmFan(gpioController, pwmController, chip, channel, hertz, tachoPin))
                {
                    fan.On();
                    Task.Delay(new TimeSpan(0, 0, 0, 2)).Wait(); //Wait for a bit for inital fan start up.
                    var dutyCycle = 100;
                    while (dutyCycle >= 0)
                    {
                        fan.SetSpeed(dutyCycle);
                        Task.Delay(new TimeSpan(0, 0, 0, 2)).Wait(); //2 seconds
                        Console.WriteLine($"DutyCycle: {dutyCycle}, RPM: {fan.RPM}");
                        dutyCycle = dutyCycle - 10;
                    }

                    fan.Off();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            finally
            {
                pwmController.Dispose();
                gpioController.Dispose();
            }
            Console.WriteLine("Finished - Real World Demo");
        }

        static void PWMFanSimpleExample()
        {
            Console.WriteLine("Starting PWM Controller - Simple Demo");
            using (var controller = new PwmController())
            {
                double dutyCycle = 100;
                var chip = 0;
                var channel = 0;
                var hertz = 25;
                controller.OpenChannel(chip, channel);
                controller.StartWriting(chip, channel, hertz, dutyCycle);
                Console.WriteLine("Duty cycle " + dutyCycle);
                Task.Delay(new TimeSpan(0, 0, 10)).Wait(); //10 second wait to give fan time to power up
                ReadTachometer();

                dutyCycle = 70;
                controller.ChangeDutyCycle(chip, channel, dutyCycle);
                Console.WriteLine("Duty cycle " + dutyCycle);
                Task.Delay(new TimeSpan(0, 0, 2)).Wait(); //2 second wait
                ReadTachometer();

                dutyCycle = 30;
                controller.ChangeDutyCycle(chip, channel, dutyCycle);
                Console.WriteLine("Duty cycle " + dutyCycle);
                Task.Delay(new TimeSpan(0, 0, 2)).Wait(); //2 second wait
                ReadTachometer();

                controller.ChangeDutyCycle(chip, channel, 0); //
                controller.StopWriting(chip, channel);
                controller.CloseChannel(chip, channel);
                Console.WriteLine("Finished - Simple Demo");
            }
        }

        static void ReadTachometer()
        {
            var pin = 23;
            var pulses = 0;
            var startTime = DateTime.Now;
            var sampleMilliseconds = 5000; 
            PinChangeEventHandler onPinEvent = (object sender, PinValueChangedEventArgs args) => { pulses++; };
            using (var controller = new GpioController())
            {
                controller.OpenPin(pin, PinMode.InputPullUp);
                controller.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Rising, onPinEvent);
                Task.Delay(new TimeSpan(0, 0, 0, 0, sampleMilliseconds)).Wait(); //wait
                var milliSeconds = (DateTime.Now - startTime).TotalMilliseconds;
                var revsPerSecond = (pulses / 2) / (milliSeconds / 1000);
                var rpm = Convert.ToInt32(revsPerSecond * 60);
                controller.UnregisterCallbackForPinValueChangedEvent(pin, onPinEvent);
                Console.WriteLine($"Fan is running at {rpm} revolutions per minute");
            }
        }
    }
}
