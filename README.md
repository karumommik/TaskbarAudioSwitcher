# Taskbar Audio Switcher (Quick Audio Output and Volume Selection on the Taskbar)

This is an extremely lightweight, stable, and convenient Windows 11 utility that automatically places itself on the taskbar (next to the system clock and system tray icons), allowing you to control all computer audio quickly and comfortably.

### Preview
![Taskbar Widget](TAS1.png)  
![Expanded Mixer Panel](TAS2.png)

## Key Features
1. **Dynamic Audio Device Switching:** Easily cycle between audio outputs (e.g. Speakers, Headphones, HDMI) directly from your taskbar. Active icons display custom 3-letter nicknames or system abbreviations underneath. There is no hardcoded device limit – display as many outputs as you want!
2. **Dynamic Discovery & Filtering:** By default, the utility automatically discovers and displays all active/enabled Windows audio devices (e.g. newly connected Bluetooth headphones appear instantly). You can toggle a filter in settings to display only a manually checked checklist of outputs.
3. **Volume Regulation & Step Customization:** Drag the slider or scroll your mouse wheel anywhere over the utility bar to adjust the master volume. Customize your preferred scroll step size in settings (choose between 1%, 2%, 5%, or 10%).
4. **App Volume Mixer:** Expand a dynamic, lightweight application volume mixer right above the taskbar. Its height automatically scales dynamically based on the monitor screen boundaries.
   - **Individual App Mouse-Wheel Control:** Scroll directly over a specific row to adjust only that application's volume, without changing the master volume.
   - **Hide Silent Apps:** Hide inactive/silent background audio sessions with a single checkbox to keep the mixer panel compact and clean.
5. **Quick Monitor Switcher Button:** An optional on-bar button (toggled via settings) shifts the utility across screens. Left-click moves the utility to the next monitor to the right (placed on its left edge). Right-click moves it to the next monitor to the left (placed on its right edge). The alignment and monitor selection are saved instantly and persist across reboots.
6. **High-DPI Per-Monitor Awareness:** Built-in dynamic DPI scale tracking. The entire widget (buttons, sliders, separators, custom fonts) automatically redraws and resizes on the fly to remain razor-sharp on screens with different scaling factors (e.g. 100%, 125%, 150%, 200%).
7. **Configuration View:** Accessible via the system tray icon, allowing you to:
   - Toggle manual device filtering and assign custom 3-letter nicknames for each device.
   - Align the widget next to the clock (Right) or next to the Start button (Left).
   - Toggle "Always on Top" and "Move to second screen on game launch" (processes stay locked to the second screen until exit).
   - Toggle the monitor switch button visibility and configure scroll step sizes.

## Release History & Changelog

### v2.0.0 (Current Version)
This major release modernizes the application's framework and architecture, transforming it from a legacy .NET 4.0 monolith to a modular .NET 10.0 project with standalone build support.
* **Modern .NET 10.0 Migration:** Replaced the obsolete .NET Framework 4.0 target. The application now runs on CoreCLR, benefiting from modern runtime speed, optimized Garbage Collection (GC) to minimize taustal/background CPU ticks, and native COM interop performance.
* **Modular Codebase Architecture:** Decomposed the massive 3000-line single-file monolith (`Program.cs`) into a clean, object-oriented directory structure:
  - `Native/` for Core Audio COM and Win32 P/Invokes.
  - `Core/` for application settings and helper utilities.
  - `Controls/` for custom drawn controls (`IconButton`, `VolumeSlider`).
  - `UI/` for layout forms (`AudioWidgetForm`, `SettingsForm`).
* **Self-Contained Publish Support:** Created a modern build pipeline via `build.bat` that bundles the .NET runtime into a single, optimized, standalone executable. Users no longer need to compile locally with raw compiler hacks or have the .NET runtime preinstalled.

### v1.3.0
This major update introduces device custom nicknames, interactive volume scroll step settings, silent application filtering, a dedicated monitor switch button, and full high-DPI scaling.
* **Device Custom Nicknames:** Added small textboxes next to the device checkboxes in the settings dialog, allowing you to define custom 3-letter nicknames/abbreviations for your output devices (e.g. `KLA` for klapid, `MÄN` for gaming headphones). If left empty, it falls back to the first 3 letters of the system name.
* **Hide Silent Apps in Mixer:** Added a "Hide silent apps" checkbox directly at the top of the expanded mixer panel. When checked, it filters out applications whose WASAPI audio session states are currently inactive, automatically resizing the mixer window to save space.
* **Customizable Scroll Volume Step:** Added a dropdown in the settings panel to change the mouse wheel scroll step size (choose between 1%, 2%, 5%, or 10%).
* **Monitor Switch Button (Left/Right Clicks):** Added an optional monitor-switching button next to the mixer button (configurable via settings). Left-clicking shifts the utility to the next physical monitor to the right (automatically docking to its left edge). Right-clicking shifts it to the next monitor to the left (docking to its right edge). The chosen position is automatically saved and persists across app restarts and reboots.
* **High-DPI Per-Monitor Awareness:** Implemented custom scaling inside the layout manager and custom drawing controls, ensuring the entire widget (buttons, volume sliders, fonts, separators) resizes and draws razor-sharp on screens with different DPI scaling factors (e.g., 100%, 125%, 150%, 200%).

### v1.2.0
This release introduces smart dynamic audio output filtering and automatic discovery.
* **Dynamic Audio Device Filtering (Toggle):** Added a new setting ("Show only selected devices (filter active)"). When unchecked (default), the utility automatically displays **all active/enabled audio outputs** in Windows. When checked, it applies your custom selection filter. This allows newly connected devices (like Bluetooth headphones) to be discovered and displayed automatically on the taskbar.

### v1.1.0
This release focuses on UI flexibility, individual application volume controls, and self-healing monitor bindings.
* **Dynamic Audio Device Limits:** Removed the hardcoded limit of 3 audio devices. You can now select as many outputs as you want in the settings panel. The utility bar on the taskbar dynamically resizes to fit all of them.
* **Device Name Abbreviations:** Each device icon now displays its first 3 letters as a capitalized abbreviation underneath the icon (e.g. `SPE` for Speakers, `HEA` for Headphones, `HDM` for HDMI). This makes it easy to distinguish outputs that share the same system icon.
* **Stable Monitor Binding & Migration:** Added auto-migration code that automatically saves the screen device name (`ScreenDeviceName`, like `\\.\DISPLAY2`) on launch. This resolves issues where the utility would jump back to the primary (left) screen during system wakeup or display layout renegotiations.
* **Mouse-Wheel App Routing:** Scrolling your mouse wheel directly over a row in the expanded mixer adjusts *only* that application's volume without affecting the master volume.
* **Horizontal Scrollbar Elimination:** Redesigned mixer row layouts to dynamically calculate control sizing relative to parent panel bounds, completely eliminating unwanted horizontal scrollbars.
* **Dynamic Mixer Sizing:** The mixer panel's height is now constrained dynamically based on the current screen's vertical bounds (`screen_height - 120` px), preventing the mixer window from going off-screen while maximizing usable layout height.
* **Locked Fullscreen Jumps:** Modified fullscreen monitor detection to associate with process IDs (`activeFullscreenProcessId`). The utility stays locked on the secondary monitor as long as the target fullscreen process is running, even if the game gets minimized or temporarily loses focus.

### v1.0.0 (Initial Release)
The first baseline release of Taskbar Audio Switcher.
* Core Win32 overlay positioning and taskbar alignment.
* Real-time Windows default audio output switching.
* Basic volume control slider and instant mute button.
* Simple App Volume Mixer using WASAPI COM interfaces.
* Automatic Light/Dark mode theme detection.
* Memory optimizations (explicit COM releasing and background garbage collection sweeps) keeping RAM usage under 15 MB.

## Features and Design
- **Automatic Alignment:** Dynamically detects the system clock/tray position and aligns itself on the taskbar accordingly.
- **Automatic Theme Support:** Detects Windows Light/Dark mode and adjusts the utility and mixer colors and borders accordingly (removing distracting purple tones).
- **Non-activating Window (WS_EX_NOACTIVATE):** Clicking on the utility does not steal focus from other active windows, making it perfect for gamers.
- **System Tray Icon:** Runs quietly in the background. Right-clicking the system tray icon allows you to exit the utility, open settings, or enable run at Windows startup.

## Known Behaviors & Quirks
To ensure 24/7 stability and prevent being flagged by antivirus software, this utility runs as a lightweight, borderless Win32 overlay window rather than hooking deep into the Windows Shell (`explorer.exe`) memory or modifying system system files. 

As a result, you might notice some specific behaviors:
* **Brief Vanishing/Reappearing:** When minimizing windows, pressing `Win+D` (Show Desktop), opening the Start Menu, or clicking taskbar flyouts, the utility may briefly disappear for a fraction of a second. This happens because Windows resets the overlay order of taskbar windows. The utility automatically detects this and safely repositions itself back into place on the next background timer tick (within 500ms).
* **Focus Safety:** Clicking the utility or adjusting volumes does not steal focus from your active windows or games, meaning you won't be accidentally tabbed out of your active application.

## Installation and Running
### 1. Compile and Run
To compile the application, you need the **.NET 10.0 SDK** (or higher) installed on your machine.
- Double-click the `build.bat` file in the folder.
- This script checks for the .NET SDK and runs a self-contained single-file publish (`dotnet publish`).
- The optimized executable `TaskbarAudioSwitcher.exe` will be generated in the `build/` directory.
### 2. Auto-start with Windows
- Right-click the blue speaker icon in the system tray.
- Select **"Run at Windows Startup"**. This adds the program to the registry (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
### 3. Exit
- Right-click the blue icon in the system tray and select **"Exit"**.
