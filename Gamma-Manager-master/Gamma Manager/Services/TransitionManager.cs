using System;
using System.Windows.Forms;
using Gamma_Manager.Models;
using Gamma_Manager;

namespace Gamma_Manager.Services
{
    internal class TransitionManager
    {
        private Timer _timer;
        private Display.DisplayInfo _targetDisplay;
        private Preset _targetPreset;
        private Preset _startPreset;
        private int _steps;
        private int _currentStep;
        private Action _updateCallback;

        public TransitionManager()
        {
            _timer = new Timer();
            _timer.Interval = 16; // ~60fps
            _timer.Tick += Timer_Tick;
        }

        public void StartTransition(Display.DisplayInfo display, Preset target, Action updateCallback)
        {
            _timer.Stop();

            _targetDisplay = display;
            _targetPreset = target;
            _updateCallback = updateCallback;

            // Capture current state as start preset
            _startPreset = new Preset
            {
                RGamma = display.rGamma,
                GGamma = display.gGamma,
                BGamma = display.bGamma,
                RContrast = display.rContrast,
                GContrast = display.gContrast,
                BContrast = display.bContrast,
                RBright = display.rBright,
                GBright = display.gBright,
                BBright = display.bBright
            };

            _steps = 30; // 0.5 seconds approx
            _currentStep = 0;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _currentStep++;
            float t = (float)_currentStep / _steps;

            // Ease out cubic
            t = 1 - (float)Math.Pow(1 - t, 3);

            if (_currentStep >= _steps)
            {
                ApplyValues(_targetPreset);
                _timer.Stop();
            }
            else
            {
                Preset current = Lerp(_startPreset, _targetPreset, t);
                ApplyValues(current);
            }

            _updateCallback?.Invoke();
        }

        private void ApplyValues(Preset p)
        {
            _targetDisplay.rGamma = p.RGamma;
            _targetDisplay.gGamma = p.GGamma;
            _targetDisplay.bGamma = p.BGamma;
            _targetDisplay.rContrast = p.RContrast;
            _targetDisplay.gContrast = p.GContrast;
            _targetDisplay.bContrast = p.BContrast;
            _targetDisplay.rBright = p.RBright;
            _targetDisplay.gBright = p.GBright;
            _targetDisplay.bBright = p.BBright;

            // Note: Monitor Hardware Brightness/Contrast usually shouldn't be animated quickly as it's slow DDC/CI
            // So we only animate Gamma/Software Contrast/Brightness
            
             Gamma.SetGammaRamp(_targetDisplay.displayLink,
                        Gamma.CreateGammaRamp(_targetDisplay.rGamma, _targetDisplay.gGamma, _targetDisplay.bGamma, 
                        _targetDisplay.rContrast, _targetDisplay.gContrast, _targetDisplay.bContrast, 
                        _targetDisplay.rBright, _targetDisplay.gBright, _targetDisplay.bBright));
        }

        private Preset Lerp(Preset a, Preset b, float t)
        {
            return new Preset
            {
                RGamma = a.RGamma + (b.RGamma - a.RGamma) * t,
                GGamma = a.GGamma + (b.GGamma - a.GGamma) * t,
                BGamma = a.BGamma + (b.BGamma - a.BGamma) * t,
                RContrast = a.RContrast + (b.RContrast - a.RContrast) * t,
                GContrast = a.GContrast + (b.GContrast - a.GContrast) * t,
                BContrast = a.BContrast + (b.BContrast - a.BContrast) * t,
                RBright = a.RBright + (b.RBright - a.RBright) * t,
                GBright = a.GBright + (b.GBright - a.GBright) * t,
                BBright = a.BBright + (b.BBright - a.BBright) * t
            };
        }
    }
}
