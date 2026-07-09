# Privacy Policy for Taskbar Audio Switcher

**Effective Date:** July 9, 2026

This Privacy Policy explains how **Taskbar Audio Switcher** ("the Application") handles your information. 

## 1. No Personal Data Collection
Taskbar Audio Switcher is designed to run entirely locally on your machine. 
* **No Information Collected:** The Application does not collect, harvest, store, or transmit any personal data, personally identifiable information (PII), or device-specific telemetry.
* **No Account Required:** You do not need to create an account or provide any email address, name, or other identifier to use the Application.

## 2. Permissions and Local Data Processing
To function properly, the Application requests standard Windows system capabilities:
* **Audio Device Management (runFullTrust):** Used solely to interact with lokaalsete core Windows Audio APIs (WASAPI) and COM interfaces to display your default audio outputs/inputs and let you switch or mute them.
* **Volume Regulation:** Used solely to adjust individual application volumes and master volume on your local system.
* **All processing is local:** None of the audio device listings, volume values, or configuration settings are ever sent to any remote server.

## 3. Network Connections
* **Microsoft Store Version:** The packaged Store version of the Application runs 100% offline and does not establish any network connections.
* **Standalone/Portable Version:** The portable version makes a single HTTPS request directly to the public GitHub API on startup to check for newer releases. No personal identifiers or usage statistics are transmitted during this version check.

## 4. Third-Party Services
The Application does not include any ads, tracking SDKs, or third-party analytics libraries.

## 5. Contact
If you have any questions or feedback, you can contact the developer by opening an issue on the official GitHub repository:
[https://github.com/karumommik/TaskbarAudioSwitcher](https://github.com/karumommik/TaskbarAudioSwitcher)
