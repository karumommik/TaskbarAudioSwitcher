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

## Recent Improvements and Fixes (Updates)
Several critical low-level Windows API and memory improvements have been made to the project in recent development cycles to ensure 24/7 stability:
* **Full Audio Mixer Functionality and COM V-table Fix:** Fixed a typo in the `IAudioSessionControl` IID GUID and added the missing `SetDisplayName` method. This synchronized the code perfectly with the low-level Windows COM virtual table (v-table), resolving incorrect Process ID retrieval and ensuring application names, icons, and individual volumes are always loaded and saved correctly.
* **Troubleshooting and Fallback ID Support:** All WASAPI interfaces now use the `[PreserveSig]` attribute. The program no longer crashes with unexpected exceptions if a temporary audio session returns error `0x80070490` (Element not found) during search; instead, it generates a safe fallback ID and successfully continues loading the mixer.
* **Smart Mixer Auto-Close:** The mixer now closes using a combination of the `GetForegroundWindow` API and mouse position tracking. The mixer stays open steadily as long as the user's mouse is over it, and closes/collapses in a fraction of a second as soon as the user moves the mouse away and clicks elsewhere, or when Windows hides the utility behind the taskbar.
* **Memory Leak Fix and GC Optimization:** Resolved a COM object memory leak by correctly releasing all activated audio output COM references (`SafeRelease`). Additionally, the program now performs automatic Garbage Collection in the background every 60 seconds and releases excess resources immediately upon closing the mixer. Memory usage remains stable between **13.5 - 14.5 MB**.

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
