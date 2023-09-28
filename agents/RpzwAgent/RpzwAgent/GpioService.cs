using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace RpzwAgent
{
    public class GpioService
    {
        private static object _pinValueChangedLock = new object();
        private List<int> _inPins = new List<int>() { 23, 24, 5, 6, 13 };
        private List<int> _outPins = new List<int>() { 18 };
        private DateTimeOffset[] _pinStateLastChangeUp = new DateTimeOffset[5];
        private DateTimeOffset[] _pinStateLastChangeDown = new DateTimeOffset[5];
        private Action<int, bool> _onPinStateChange;
        private CancellationTokenSource _monitorCts = new CancellationTokenSource();

        public GpioController GpioController { get; }

        public GpioService()
        {
            GpioController = new GpioController();
        }

        public void Run(Action<int, bool> onPinStateChange)
        {
            _onPinStateChange = onPinStateChange;
            foreach (var outPin in _inPins)
            {
                OpenInPin(outPin);
            }
            foreach (var outPin in _outPins)
            {
                OpenOutPin(outPin);
            }

            Task.Run(async () =>
            {
                _monitorCts.Cancel();
                _monitorCts = new CancellationTokenSource();
                var cancellationToken = _monitorCts.Token;
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    Console.WriteLine();
                    Console.WriteLine(string.Join("\t", _inPins));
                    Console.WriteLine(string.Join("\t",
                        GpioController.Read(_inPins[0]) == PinValue.High,
                        GpioController.Read(_inPins[1]) == PinValue.High,
                        GpioController.Read(_inPins[2]) == PinValue.High,
                        GpioController.Read(_inPins[3]) == PinValue.High,
                        GpioController.Read(_inPins[4]) == PinValue.High));
                    await Task.Delay(250);
                }
            });
        }

        public void SetPin(int pin, bool high)
        {
            GpioController.Write(18, high ? PinValue.High : PinValue.Low);
        }

        private void OpenInPin(int pin)
        {
            if (GpioController.IsPinOpen(pin))
            {
                GpioController.ClosePin(pin);
            }
            GpioController.OpenPin(pin, PinMode.InputPullDown);
            GpioController.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Rising, OnPinValueChanged);
            GpioController.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Falling, OnPinValueChanged);
        }

        private void OpenOutPin(int pin)
        {
            if (GpioController.IsPinOpen(pin))
            {
                GpioController.ClosePin(pin);
            }
            GpioController.OpenPin(18, PinMode.Output);
        }

        private void OnPinValueChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            //Console.WriteLine($"{pinValueChangedEventArgs.PinNumber}: {pinValueChangedEventArgs.ChangeType}");
            lock (_pinValueChangedLock)
            {
                var index = _inPins.IndexOf(pinValueChangedEventArgs.PinNumber);
                if (pinValueChangedEventArgs.ChangeType == PinEventTypes.Rising &&
                    _pinStateLastChangeUp[index] + TimeSpan.FromSeconds(1) < DateTimeOffset.UtcNow)
                {
                    _pinStateLastChangeUp[index] = DateTimeOffset.UtcNow;
                    Task.Run(async () =>
                    {
                        await Task.Delay(33);
                        if (GpioController.Read(pinValueChangedEventArgs.PinNumber) == PinValue.High)
                        {
                            _onPinStateChange(pinValueChangedEventArgs.PinNumber, true);
                        }
                    });
                }
                if (pinValueChangedEventArgs.ChangeType == PinEventTypes.Falling &&
                    _pinStateLastChangeDown[index] + TimeSpan.FromSeconds(1) < DateTimeOffset.UtcNow)
                {
                    _pinStateLastChangeDown[index] = DateTimeOffset.UtcNow;
                    Task.Run(async () =>
                    {
                        await Task.Delay(33);
                        if (GpioController.Read(pinValueChangedEventArgs.PinNumber) == PinValue.Low)
                        {
                            _onPinStateChange(pinValueChangedEventArgs.PinNumber, false);
                        }
                    });
                }
            }
        }
    }
}
