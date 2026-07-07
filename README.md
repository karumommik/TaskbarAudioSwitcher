# Taskbar Audio Switcher (Quick Audio Output and Volume Selection on the Taskbar)

This is an extremely lightweight, stable, and convenient Windows 11 utility that automatically places itself on the taskbar (next to the system clock and system tray icons), allowing you to control all computer audio quickly and comfortably.

### Preview
![Taskbar Widget](TAS1.png)  
![Expanded Mixer Panel](TAS2.png)

## Key Features
1. **Quick Audio Device Switching:** Switch between up to 3 selected active devices (e.g. speakers, headphones, TV) by clicking on their corresponding icons.
2. **Volume Regulation:** Adjust volume by dragging the slider or scrolling the mouse wheel directly over the utility.
3. **Mute/Unmute:** Mute audio with a single click.
4. **App Volume Mixer:** Clicking the button next to the volume percentage opens a dynamic mixer above the utility, allowing you to regulate the volume of each audio-producing application (e.g., Chrome, Firefox, Spotify) individually or mute them.
5. **Configuration View:** The settings window, opened from the system tray icon, allows you to:
   - Select and filter which audio devices are displayed on the utility bar.
   - Choose whether the utility is displayed to the left or right of the taskbar clock.
   - Enable "Always on Top" mode to keep the utility visible even over fullscreen games/applications.
   - Activate a mode that automatically moves the utility to the left edge of the second monitor when a fullscreen or borderless windowed game or application is launched, and returns it to its original position upon exiting the game.

## Release History & Changelog

### v1.1.0 (Current Version)
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
Double-click the convenient `compile.bat` file included in the folder.
- This compiles the C# code directly using the local built-in Windows C# compiler (`csc.exe`).
- The `TaskbarAudioSwitcher.exe` file will be created in the same folder and launched automatically.
### 2. Auto-start with Windows
- Right-click the blue speaker icon in the system tray.
- Select **"Run at Windows Startup"**. This adds the program to the registry (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
### 3. Exit
- Right-click the blue icon in the system tray and select **"Exit"**.
