# 🚀 Lexplosion — Modern Minecraft Launcher

[![License: Custom](https://img.shields.io/badge/License-Proprietary-red.svg)](LICENSE)
[![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.7.2-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-blue?logo=windows)](https://github.com/dotnet/wpf)

**Lexplosion** is a desktop Minecraft launcher designed to consolidate the most effective features of modern platforms while introducing unique, built-in network and customization capabilities. 

The goal of the project is to provide a unified, highly optimized environment where users can manage multiple game profiles, easily search and install modifications from popular databases, and play multiplayer sessions without complex configuration.

> [!IMPORTANT]
> All code in this repository is provided strictly for educational purposes or to allow contributors to propose improvements. Any unauthorized modification or redistribution of the source code in this repository is prohibited. By submitting a Pull Request that is subsequently accepted and merged, you agree to transfer all proprietary and intellectual property rights of the contributed code to the project maintainers.

### ✨ Features

The application is structured to support flexible modding, reliable asset downloading, and simplified network play:

- 📦 **Multi-Instance Manager**
  - Full support for all vanilla game versions, snapshots, and major modloaders (Forge, NeoForge, Fabric, Quilt).
  - Quick instance cloning and duplication.
  - Isolated configuration settings, directories, and Java parameters for each client profile.
- 🗂 **Modpack & Mod Catalogs:** Built-in integration with **CurseForge** and **Modrinth** APIs, allowing users to browse, download, and update modpacks, individual mods, resource packs, shaders, and maps directly from the UI.
- 🌐 **Simplified Multiplayer:** Built-in peer-to-peer or server assistance to facilitate multiplayer connectivity without requiring dedicated hosting platforms or external VPN utilities.
- 📤 **Export, Import & Share:** Streamlined tools to pack, export, and distribute custom setups to other players.
- ☕ **Automated JRE Management:** Automated detection, downloading, and mapping of the correct Java Runtime Environment (JRE) version required for the chosen Minecraft build.
- 🎨 **Deep Customization:** Extensively customizable interface themes, account management options (supporting multiple profile types), and in-game performance optimization tweaks.

### 🛠 Tech Stack & Architecture

The desktop application is built using a decoupled architecture, dividing core launcher logic from the user interface presentation layer.

**Core Subsystem (`Lexplosion.Core`):**
* **C# / .NET Framework 4.7.2** — Designed as a class library handling manifest parsing, parallel asset downloading, game process launching, and configuration management.
* **Libraries:** Uses `LumiSoft.Net` for networking tasks, `Tommy` for TOML configuration parsing, and `Newtonsoft.Json`.
* **Packaging:** Includes an automated post-build script that archives the compiled core DLL into `Lexplosion.Core.zip` for consumption by the UI module.

**Frontend Subsystem (`Lexplosion.UI.WPF`):**
* **WPF — The desktop user interface application.
* **Libraries:** 
  * `Hardcodet.NotifyIcon.Wpf` for system tray control.
  * `DiscordRichPresence` for game status integration.
  * `MarkdownWPF` for rendering dynamic text documents.
  * `VirtualizingWrapPanel` for highly optimized, smooth item rendering in directories and mod list grids.
* **Build Dependency:** Integrates a pre-build step that pulls and unzips the compiled `Lexplosion.Core` library directly into the application's runtime directories.
