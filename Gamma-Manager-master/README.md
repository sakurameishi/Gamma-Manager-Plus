# Gamma Manager

*Based on the original work by [KrasnovM/Gamma-Manager](https://github.com/KrasnovM/Gamma-Manager).*

**Gamma Manager** is a lightweight, open-source Windows application designed to give you complete control over your screen's gamma, brightness, and contrast. It now features a robust preset management system with global hotkeys and smooth transition animations.

![Gamma Manager Screenshot](GammaManager.jpg?raw=true)

## Key Features

*   **Comprehensive Color Control**: Adjust Gamma, Brightness, and Contrast for all channels (RGB) simultaneously or individually.
*   **Preset Management**: Create, save, and manage unlimited presets for different scenarios (e.g., "Gaming", "Night Mode", "Movie", "Coding").
*   **Global Hotkeys**: Bind custom global hotkeys (e.g., `F2`, `Ctrl+Alt+1`) to any preset for instant switching, even when the application is minimized or in the tray.
*   **Smooth Transitions**: Enjoy buttery-smooth transition animations (~0.5s) when switching between presets, preventing sudden flashes or jarring color shifts.
*   **Multi-Monitor Support**: Independently manage settings for multiple monitors.
*   **Hardware Control (DDC/CI)**: Supports adjusting physical monitor brightness and contrast for compatible external displays.
*   **System Tray Integration**: Minimizes to the system tray for unobtrusive background operation. Quick access to presets via the tray context menu.
*   **Portable & Lightweight**: No installation required. Settings are saved in a JSON file in your AppData folder.

## Installation

1.  Download the latest release from the Releases page (if available) or build from source.
2.  Run `Gamma Manager.exe`.
3.  No installation is required.

## Usage Guide

### 1. Basic Color Adjustments
*   **Sliders**: Drag the **Gamma**, **Brightness**, or **Contrast** sliders to adjust the image properties.
*   **Channels**: Click **Red**, **Green**, or **Blue** to adjust only that specific color channel. Click **All Colors** to link them back together.
*   **Monitor Controls**: Use the **Monitor Brightness/Contrast** sliders to adjust the physical hardware settings of your monitor (if supported).

### 2. Creating and Managing Presets
Save your favorite settings to switch between them instantly.

*   **Create a Preset**:
    1.  Adjust the sliders to your desired look.
    2.  Type a name for your preset in the text box (located between the "Forward" and "Save" buttons).
    3.  Click the **Save** button.
*   **Delete a Preset**:
    1.  Select a preset from the dropdown list.
    2.  Click the **Delete** button.

### 3. Setting Up Hotkeys (New!)
Bind keyboard shortcuts to your presets for instant switching while in-game or working.

1.  Select the preset you want to bind from the dropdown list.
2.  Click the **Bind Hotkey** button at the bottom-left of the window.
3.  Press the key or combination you want to use (e.g., `F2`, `Ctrl+Shift+G`).
4.  The button text will update to show your bound key.
5.  **Click "Save"** to confirm and save the binding.

### 4. System Tray
*   **Minimize**: Click the "Hide" button or minimize the window to send it to the system tray.
*   **Quick Switch**: Right-click the tray icon to see a list of your monitors and their presets for quick selection.
*   **Restore**: Double-click the tray icon to open the main window.

## Requirements
*   Windows 7, 8, 10, or 11
*   .NET Framework 4.7.2 or later

## Building from Source

To build the project locally, you need Visual Studio 2019/2022 with the .NET Desktop Development workload.

1.  Clone this repository.
2.  Open `Gamma Manager.sln` in Visual Studio.
3.  Restore NuGet packages.
4.  Build the solution (Release mode recommended).

## License
This project is licensed under the [CC0 1.0 Universal](LICENSE.txt) (Public Domain Dedication).
Original project by KrasnovM.
