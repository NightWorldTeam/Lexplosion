# 🚀 Lexplosion — Modern Minecraft Launcher

[![License: Custom](https://img.shields.io/badge/License-Proprietary-red.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-blue?logo=windows)](https://github.com/dotnet/wpf)

**Lexplosion** is a feature-rich desktop Minecraft launcher designed to consolidate the most effective features of modern platforms while introducing unique, built-in network and customization capabilities. 

The goal of the project is to provide a unified, highly optimized environment where users can manage multiple game profiles, easily search and install modifications from popular databases, and play multiplayer sessions without complex configuration.

> [!IMPORTANT]
> All code in this repository is provided strictly for educational and informational purposes. Any modification, adaptation, or redistribution of the source code in this repository is prohibited.

## ✨ Features

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

## 🛠 Tech Stack & Architecture

The desktop application is built using a clean separation of concerns, dividing core launcher logic from the user interface presentation layer.

**Core Subsystems:**
* **C# / .NET** — Powering `Lexplosion.Core`, the backend engine of the launcher responsible for manifest parsing, parallel asset downloading, game process launching, and file integrity validation.
* **WPF (Windows Presentation Foundation)** — The desktop user interface implementation layer (`Lexplosion.UI.WPF`), configured to provide a responsive, themeable layout on Windows systems.

**Auxiliary Utilities:**
* [Lexplosion UpdateTool](https://github.com/NightWorldTeam/Lexplosion-UpdateTool) — An independent background utility responsible for managing launcher updates and checking build integrity.
