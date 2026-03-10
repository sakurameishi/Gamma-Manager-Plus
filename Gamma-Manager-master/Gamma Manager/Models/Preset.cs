using System;

namespace Gamma_Manager.Models
{
    public class Preset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MonitorName { get; set; }
        public int ShortcutKey { get; set; } // Virtual Key Code
        public int ShortcutModifiers { get; set; } // 0=None, 1=Alt, 2=Ctrl, 4=Shift, 8=Win

        // Gamma
        public float RGamma { get; set; }
        public float GGamma { get; set; }
        public float BGamma { get; set; }

        // Contrast
        public float RContrast { get; set; }
        public float GContrast { get; set; }
        public float BContrast { get; set; }

        // Brightness
        public float RBright { get; set; }
        public float GBright { get; set; }
        public float BBright { get; set; }

        // Monitor Hardware Settings
        public int MonitorBrightness { get; set; }
        public int MonitorContrast { get; set; }

        public Preset()
        {
            Id = Guid.NewGuid().ToString();
            RGamma = 1.0f; GGamma = 1.0f; BGamma = 1.0f;
            RContrast = 1.0f; GContrast = 1.0f; BContrast = 1.0f;
            RBright = 0.0f; GBright = 0.0f; BBright = 0.0f;
            MonitorBrightness = 100;
            MonitorContrast = 50;
        }
    }
}
