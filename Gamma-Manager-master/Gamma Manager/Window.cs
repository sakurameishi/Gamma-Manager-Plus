using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Gamma_Manager.Models;
using Gamma_Manager.Services;

namespace Gamma_Manager
{
    public partial class Window : Form
    {
        System.Globalization.CultureInfo customCulture;
        PresetManager presetManager;
        HotkeyManager hotkeyManager;
        TransitionManager transitionManager;

        List<Display.DisplayInfo> displays = new List<Display.DisplayInfo>();
        int numDisplay = 0;
        Display.DisplayInfo currDisplay;

        List<ToolStripComboBox> toolMonitors = new List<ToolStripComboBox>();
        ToolStripComboBox toolMonitor;

        bool disableChangeFunc = false;
        bool isBindingKey = false;
        Keys currentBoundKey = Keys.None;

        bool allColors = true;
        bool redColor = false;
        bool greenColor = false;
        bool blueColor = false;

        public Window()
        {
            InitializeComponent();
            customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ",";

            presetManager = new PresetManager();
            hotkeyManager = new HotkeyManager(this.Handle);
            transitionManager = new TransitionManager();

            buttonAllColors.Font = new Font(buttonAllColors.Font.Name, buttonAllColors.Font.Size, FontStyle.Bold);

            displays = Display.QueryDisplayDevices();
            displays.Reverse();
            for (int i = 0; i < displays.Count; i++)
            {
                displays[i].numDisplay = i;
                comboBoxMonitors.Items.Add(i + 1 + ") " + displays[i].displayName);
            }
            if (displays.Count > 0)
            {
                currDisplay = displays[numDisplay];
                comboBoxMonitors.SelectedIndex = numDisplay;
                fillInfo(currDisplay);
            }

            initPresets();
            initTrayMenu();
            notifyIcon.ContextMenuStrip = contextMenu;
            
            this.KeyPreview = true;
            this.KeyDown += Window_KeyDown;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312) // WM_HOTKEY
            {
                hotkeyManager.ProcessMessage(m);
            }
            base.WndProc(ref m);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isBindingKey)
            {
                currentBoundKey = e.KeyData;
                string keyName = new KeysConverter().ConvertToString(currentBoundKey);
                labelBoundKey.Text = keyName;
                buttonBindKey.Text = "Bind Hotkey";
                isBindingKey = false;
                e.Handled = true;
            }
        }

        private void clearColors()
        {
            buttonAllColors.Font = new Font(buttonAllColors.Font.Name, buttonAllColors.Font.Size, FontStyle.Regular);
            buttonRed.Font = new Font(buttonRed.Font.Name, buttonRed.Font.Size, FontStyle.Regular);
            buttonGreen.Font = new Font(buttonGreen.Font.Name, buttonGreen.Font.Size, FontStyle.Regular);
            buttonBlue.Font = new Font(buttonBlue.Font.Name, buttonBlue.Font.Size, FontStyle.Regular);

            allColors = false;
            redColor = false;
            greenColor = false;
            blueColor = false;
        }

        private void initPresets()
        {
            comboBoxPresets.Items.Clear();
            comboBoxPresets.Text = string.Empty;
            
            var presets = presetManager.GetAllPresets();
            hotkeyManager.UnregisterAll();

            foreach (var preset in presets)
            {
                // Register hotkey
                if (preset.ShortcutKey != 0)
                {
                    Keys k = (Keys)preset.ShortcutKey;
                    // Reconstruct modifiers
                    if ((preset.ShortcutModifiers & 1) != 0) k |= Keys.Alt;
                    if ((preset.ShortcutModifiers & 2) != 0) k |= Keys.Control;
                    if ((preset.ShortcutModifiers & 4) != 0) k |= Keys.Shift;
                    if ((preset.ShortcutModifiers & 8) != 0) k |= Keys.LWin;

                    hotkeyManager.Register(k, () => ApplyPreset(preset));
                }

                if (preset.MonitorName == currDisplay.displayName)
                {
                    comboBoxPresets.Items.Add(preset.Name);
                }
            }
        }

        private void ApplyPreset(Preset preset)
        {
            // Find display for this preset
            Display.DisplayInfo targetDisplay = null;
            foreach (var d in displays)
            {
                if (d.displayName == preset.MonitorName)
                {
                    targetDisplay = d;
                    break;
                }
            }

            if (targetDisplay != null)
            {
                transitionManager.StartTransition(targetDisplay, preset, () =>
                {
                    if (currDisplay == targetDisplay)
                    {
                        this.Invoke((MethodInvoker)delegate {
                            fillInfo(currDisplay);
                        });
                    }
                });

                // Update hardware brightness/contrast immediately or after?
                // TransitionManager only does gamma. Hardware changes are slow.
                if (targetDisplay.isExternal)
                {
                     targetDisplay.monitorBrightness = preset.MonitorBrightness;
                     targetDisplay.monitorContrast = preset.MonitorContrast;
                     ExternalMonitor.SetBrightness(targetDisplay.PhysicalHandle, (uint)preset.MonitorBrightness);
                     ExternalMonitor.SetContrast(targetDisplay.PhysicalHandle, (uint)preset.MonitorContrast);
                }
                else
                {
                    targetDisplay.monitorBrightness = preset.MonitorBrightness;
                    InternalMonitor.SetBrightness((byte)preset.MonitorBrightness);
                }
                
                // If it's current display, update UI immediately for HW controls
                 if (currDisplay == targetDisplay)
                 {
                     this.Invoke((MethodInvoker)delegate {
                        fillInfo(currDisplay);
                     });
                 }
            }
        }

        private void initTrayMenu()
        {
            contextMenu.Items.Clear();
            toolMonitors.Clear();

            ToolStripMenuItem toolSetting = new ToolStripMenuItem("Settings", null, toolSettings_Click);
            contextMenu.Items.Add(toolSetting);

            ToolStripSeparator toolStripSeparator1 = new ToolStripSeparator();
            contextMenu.Items.Add(toolStripSeparator1);

            for (int i = 0; i < displays.Count; i++)
            {
                toolMonitor = new ToolStripComboBox(displays[i].displayName);
                toolMonitor.DropDownStyle = ComboBoxStyle.DropDownList;

                toolMonitor.Items.Add(displays[i].displayName + ":");
                toolMonitor.Text = displays[i].displayName + ":";

                toolMonitor.SelectedIndexChanged += new EventHandler(comboBoxToolMonitor_IndexChanged);

                var presets = presetManager.GetAllPresets();
                foreach (var preset in presets)
                {
                    if (preset.MonitorName == displays[i].displayName)
                    {
                        toolMonitor.Items.Add(preset.Name);
                    }
                }
                
                toolMonitors.Add(toolMonitor);
                contextMenu.Items.Add(toolMonitor);
            }
            ToolStripSeparator toolStripSeparator2 = new ToolStripSeparator();
            contextMenu.Items.Add(toolStripSeparator2);
            ToolStripMenuItem toolExit = new ToolStripMenuItem("Exit", null, toolExit_Click);
            contextMenu.Items.Add(toolExit);
        }

        private void fillInfo(Display.DisplayInfo currDisplay)
        {
            disableChangeFunc = true;

            textBoxGamma.Text = ((currDisplay.rGamma + currDisplay.gGamma + currDisplay.bGamma) / 3f).ToString("0.00");
            textBoxContrast.Text = ((currDisplay.rContrast + currDisplay.gContrast + currDisplay.bContrast) / 3f).ToString("0.00");
            textBoxBrightness.Text = ((currDisplay.rBright + currDisplay.gBright + currDisplay.bBright) / 3f).ToString("0.00");

            trackBarGamma.Value = (int)(((currDisplay.rGamma + currDisplay.gGamma + currDisplay.bGamma) / 3f) * 100f);
            trackBarContrast.Value = (int)(((currDisplay.rContrast + currDisplay.gContrast + currDisplay.bContrast) / 3f) * 100f);
            trackBarBrightness.Value = (int)(((currDisplay.rBright + currDisplay.gBright + currDisplay.bBright) / 3f) * 100f);

            if (currDisplay.isExternal)
            {
                labelMonitorContrastUp.Visible = true;
                labelMonitorContrastDown.Visible = true;
                trackBarMonitorContrast.Visible = true;
                textBoxMonitorContrast.Visible = true;

                // Only read from monitor if we haven't set it? 
                // Actually DisplayInfo has the cached values.
                textBoxMonitorBrightness.Text = currDisplay.monitorBrightness.ToString();
                trackBarMonitorBrightness.Value = currDisplay.monitorBrightness;

                textBoxMonitorContrast.Text = currDisplay.monitorContrast.ToString();
                trackBarMonitorContrast.Value = currDisplay.monitorContrast;
            }
            else
            {
                labelMonitorContrastUp.Visible = false;
                labelMonitorContrastDown.Visible = false;
                trackBarMonitorContrast.Visible = false;
                textBoxMonitorContrast.Visible = false;

                textBoxMonitorBrightness.Text = currDisplay.monitorBrightness.ToString();
                trackBarMonitorBrightness.Value = currDisplay.monitorBrightness;
            }
            disableChangeFunc = false;
        }

        private void Window_Load(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Size.Width;
            int windowWidth = Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Size.Height;
            int windowHeight = Height;
            int tmp = Screen.PrimaryScreen.Bounds.Height;
            int TaskBarHeight = tmp - Screen.PrimaryScreen.WorkingArea.Height;

            Location = new Point(screenWidth - windowWidth, screenHeight - (windowHeight + TaskBarHeight));
        }

        // ... TrackBar Handlers ...
        private void trackBarGamma_ValueChanged(object sender, EventArgs e)
        {
            if (disableChangeFunc) return;
            // comboBoxPresets.Text = string.Empty; // Don't clear preset name immediately, or do?

            textBoxGamma.Text = ((float)trackBarGamma.Value / 100f).ToString("0.00");
            float val = (float)trackBarGamma.Value / 100f;

            if (allColors) { currDisplay.rGamma = val; currDisplay.gGamma = val; currDisplay.bGamma = val; }
            else if (redColor) currDisplay.rGamma = val;
            else if (greenColor) currDisplay.gGamma = val;
            else if (blueColor) currDisplay.bGamma = val;

            UpdateGamma();
        }

        private void trackBarContrast_ValueChanged(object sender, EventArgs e)
        {
            if (disableChangeFunc) return;
            
            textBoxContrast.Text = ((float)trackBarContrast.Value / 100f).ToString("0.00");
            float val = (float)trackBarContrast.Value / 100f;

            if (allColors) { currDisplay.rContrast = val; currDisplay.gContrast = val; currDisplay.bContrast = val; }
            else if (redColor) currDisplay.rContrast = val;
            else if (greenColor) currDisplay.gContrast = val;
            else if (blueColor) currDisplay.bContrast = val;

            UpdateGamma();
        }

        private void trackBarBrightness_ValueChanged(object sender, EventArgs e)
        {
            if (disableChangeFunc) return;

            textBoxBrightness.Text = ((float)trackBarBrightness.Value / 100f).ToString("0.00");
            float val = (float)trackBarBrightness.Value / 100f;

            if (allColors) { currDisplay.rBright = val; currDisplay.gBright = val; currDisplay.bBright = val; }
            else if (redColor) currDisplay.rBright = val;
            else if (greenColor) currDisplay.gBright = val;
            else if (blueColor) currDisplay.bBright = val;

            UpdateGamma();
        }

        private void UpdateGamma()
        {
             Gamma.SetGammaRamp(currDisplay.displayLink,
                        Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma, currDisplay.rContrast,
                        currDisplay.gContrast, currDisplay.bContrast, currDisplay.rBright, currDisplay.gBright, currDisplay.bBright));
        }

        private void trackBarMonitorBrightness_ValueChanged(object sender, EventArgs e)
        {
            if (disableChangeFunc) return;
            textBoxMonitorBrightness.Text = trackBarMonitorBrightness.Value.ToString();
            currDisplay.monitorBrightness = trackBarMonitorBrightness.Value;
            if (currDisplay.isExternal) ExternalMonitor.SetBrightness(currDisplay.PhysicalHandle, (uint)trackBarMonitorBrightness.Value);
            else InternalMonitor.SetBrightness((byte)trackBarMonitorBrightness.Value);
        }

        private void trackBarMonitorContrast_ValueChanged(object sender, EventArgs e)
        {
            if (disableChangeFunc) return;
            textBoxMonitorContrast.Text = trackBarMonitorContrast.Value.ToString();
            currDisplay.monitorContrast = trackBarMonitorContrast.Value;
            ExternalMonitor.SetContrast(currDisplay.PhysicalHandle, (uint)trackBarMonitorContrast.Value);
        }

        private void buttonAllColors_Click(object sender, EventArgs e)
        {
            disableChangeFunc = true;
            clearColors();
            allColors = true;
            fillInfo(currDisplay); // Refresh trackbars for average
            buttonAllColors.Font = new Font(buttonAllColors.Font.Name, buttonAllColors.Font.Size, FontStyle.Bold);
            disableChangeFunc = false;
        }

        private void buttonRed_Click(object sender, EventArgs e)
        {
            disableChangeFunc = true;
            clearColors();
            redColor = true;
            
            textBoxGamma.Text = currDisplay.rGamma.ToString("0.00");
            textBoxContrast.Text = currDisplay.rContrast.ToString("0.00");
            textBoxBrightness.Text = currDisplay.rBright.ToString("0.00");
            trackBarGamma.Value = (int)(currDisplay.rGamma * 100f);
            trackBarContrast.Value = (int)(currDisplay.rContrast * 100f);
            trackBarBrightness.Value = (int)(currDisplay.rBright * 100f);

            buttonRed.Font = new Font(buttonRed.Font.Name, buttonRed.Font.Size, FontStyle.Bold);
            disableChangeFunc = false;
        }

        private void buttonGreen_Click(object sender, EventArgs e)
        {
            disableChangeFunc = true;
            clearColors();
            greenColor = true;
            
            textBoxGamma.Text = currDisplay.gGamma.ToString("0.00");
            textBoxContrast.Text = currDisplay.gContrast.ToString("0.00");
            textBoxBrightness.Text = currDisplay.gBright.ToString("0.00");
            trackBarGamma.Value = (int)(currDisplay.gGamma * 100f);
            trackBarContrast.Value = (int)(currDisplay.gContrast * 100f);
            trackBarBrightness.Value = (int)(currDisplay.gBright * 100f);

            buttonGreen.Font = new Font(buttonGreen.Font.Name, buttonGreen.Font.Size, FontStyle.Bold);
            disableChangeFunc = false;
        }

        private void buttonBlue_Click(object sender, EventArgs e)
        {
            disableChangeFunc = true;
            clearColors();
            blueColor = true;

            textBoxGamma.Text = currDisplay.bGamma.ToString("0.00");
            textBoxContrast.Text = currDisplay.bContrast.ToString("0.00");
            textBoxBrightness.Text = currDisplay.bBright.ToString("0.00");
            trackBarGamma.Value = (int)(currDisplay.bGamma * 100f);
            trackBarContrast.Value = (int)(currDisplay.bContrast * 100f);
            trackBarBrightness.Value = (int)(currDisplay.bBright * 100f);

            buttonBlue.Font = new Font(buttonBlue.Font.Name, buttonBlue.Font.Size, FontStyle.Bold);
            disableChangeFunc = false;
        }

        private void checkBoxExContrast_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxExContrast.Checked) trackBarContrast.Maximum = 10000;
            else trackBarContrast.Maximum = 300;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string name = comboBoxPresets.Text;
            if (string.IsNullOrWhiteSpace(name)) return;

            Preset preset = new Preset
            {
                Name = name,
                MonitorName = currDisplay.displayName,
                RGamma = currDisplay.rGamma,
                GGamma = currDisplay.gGamma,
                BGamma = currDisplay.bGamma,
                RContrast = currDisplay.rContrast,
                GContrast = currDisplay.gContrast,
                BContrast = currDisplay.bContrast,
                RBright = currDisplay.rBright,
                GBright = currDisplay.gBright,
                BBright = currDisplay.bBright,
                MonitorBrightness = currDisplay.monitorBrightness,
                MonitorContrast = currDisplay.monitorContrast
            };
            
            // If updating existing, preserve ID and Shortcut
            var existing = presetManager.GetAllPresets().Find(p => p.Name == name && p.MonitorName == currDisplay.displayName);
            if (existing != null)
            {
                preset.Id = existing.Id;
                preset.ShortcutKey = existing.ShortcutKey;
                preset.ShortcutModifiers = existing.ShortcutModifiers;
            }

            // If a key is currently bound in UI, use it
            if (currentBoundKey != Keys.None)
            {
                 preset.ShortcutKey = (int)(currentBoundKey & Keys.KeyCode);
                 int mods = 0;
                 if ((currentBoundKey & Keys.Alt) == Keys.Alt) mods |= 1;
                 if ((currentBoundKey & Keys.Control) == Keys.Control) mods |= 2;
                 if ((currentBoundKey & Keys.Shift) == Keys.Shift) mods |= 4;
                 if ((currentBoundKey & Keys.LWin) == Keys.LWin) mods |= 8;
                 preset.ShortcutModifiers = mods;
            }

            presetManager.UpdatePreset(preset); // Or Add if new
            initPresets();
            initTrayMenu();
            comboBoxPresets.Text = name;
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            string name = comboBoxPresets.Text;
            var preset = presetManager.GetAllPresets().Find(p => p.Name == name && p.MonitorName == currDisplay.displayName);
            if (preset != null)
            {
                presetManager.DeletePreset(preset.Id);
                initPresets();
                initTrayMenu();
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            comboBoxPresets.Text = string.Empty;
            buttonAllColors.PerformClick();

            trackBarGamma.Value = 100;
            trackBarContrast.Value = 100;
            trackBarBrightness.Value = 0;

            currDisplay.rGamma = 1; currDisplay.gGamma = 1; currDisplay.bGamma = 1;
            currDisplay.rContrast = 1; currDisplay.gContrast = 1; currDisplay.bContrast = 1;
            currDisplay.rBright = 0; currDisplay.gBright = 0; currDisplay.bBright = 0;

            if (currDisplay.isExternal)
            {
                trackBarMonitorBrightness.Value = 100;
                trackBarMonitorContrast.Value = 50;
            }
            else
            {
                trackBarMonitorBrightness.Value = 100;
            }
            UpdateGamma();
        }

        private void buttonHide_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void comboBoxMonitors_SelectedIndexChanged(object sender, EventArgs e)
        {
            string num = comboBoxMonitors.SelectedItem.ToString();
            num = num.Substring(0, num.IndexOf(")"));
            numDisplay = Int32.Parse(num) - 1;

            currDisplay = displays[numDisplay];
            fillInfo(currDisplay);
            initPresets();
            currentBoundKey = Keys.None;
            labelBoundKey.Text = "None";
        }

        private void buttonForward_Click(object sender, EventArgs e)
        {
            if (numDisplay + 1 <= displays.Count - 1)
            {
                comboBoxMonitors.SelectedIndex = numDisplay + 1;
            }
            else
            {
                comboBoxMonitors.SelectedIndex = 0;
            }
        }

        private void comboBoxPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (disableChangeFunc) return;
            string name = comboBoxPresets.SelectedItem.ToString();
            var preset = presetManager.GetAllPresets().Find(p => p.Name == name && p.MonitorName == currDisplay.displayName);
            if (preset != null)
            {
                // Update bound key label
                if (preset.ShortcutKey != 0)
                {
                     Keys k = (Keys)preset.ShortcutKey;
                     if ((preset.ShortcutModifiers & 1) != 0) k |= Keys.Alt;
                     if ((preset.ShortcutModifiers & 2) != 0) k |= Keys.Control;
                     if ((preset.ShortcutModifiers & 4) != 0) k |= Keys.Shift;
                     currentBoundKey = k;
                     labelBoundKey.Text = new KeysConverter().ConvertToString(k);
                }
                else
                {
                    currentBoundKey = Keys.None;
                    labelBoundKey.Text = "None";
                }

                // Apply with transition
                transitionManager.StartTransition(currDisplay, preset, () =>
                {
                    this.Invoke((MethodInvoker)delegate {
                        fillInfo(currDisplay);
                    });
                });
            }
        }

        private void buttonBindKey_Click(object sender, EventArgs e)
        {
            isBindingKey = true;
            buttonBindKey.Text = "Press any key...";
            this.Focus();
        }

        private void Window_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized) Hide();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void toolSettings_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void toolExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBoxToolMonitor_IndexChanged(object sender, EventArgs e)
        {
             // This logic was complex in original, simplifying to just apply preset
             // But Wait, toolMonitors contains presets for specific monitors.
             // sender is the ToolStripComboBox.
             ToolStripComboBox cb = sender as ToolStripComboBox;
             if (cb != null && cb.SelectedIndex > 0)
             {
                 string presetName = cb.SelectedItem.ToString();
                 // Find monitor by CB index in toolMonitors list?
                 int monitorIndex = toolMonitors.IndexOf(cb);
                 if (monitorIndex >= 0)
                 {
                     var display = displays[monitorIndex];
                     var preset = presetManager.GetAllPresets().Find(p => p.Name == presetName && p.MonitorName == display.displayName);
                     if (preset != null)
                     {
                         // Apply preset to that display
                         // Need to switch to that display to update UI if it's not current?
                         // If I just apply it, the UI won't update if it's not current.
                         // But the user clicked from tray, so maybe they don't care about UI.
                         // I will just apply it.
                         
                         // But TransitionManager works on 'DisplayInfo' reference.
                         // So I can use it.
                         transitionManager.StartTransition(display, preset, () => {
                             if (currDisplay == display)
                             {
                                 this.Invoke((MethodInvoker)delegate {
                                     fillInfo(currDisplay);
                                 });
                             }
                         });
                     }
                 }
             }
        }
    }
}
