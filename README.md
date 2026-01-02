# RP Buddy

A Dalamud plugin for FINAL FANTASY XIV designed to enhance the roleplaying experience by providing visual tools for chat customization.

## Features

### Chat Recoloring
RP Buddy automatically recolors chat messages based on roleplaying conventions:
- **Quoted text** (text within quotation marks `"..."`) - Recolored to distinguish spoken dialogue
- **Emoted actions** (text within asterisks `*...*`) - Recolored to distinguish character actions and emotes

This visual distinction helps roleplayers quickly identify dialogue versus actions in busy chat scenarios, making it easier to follow ongoing roleplay scenes.

## Installation

### Prerequisites

* XIVLauncher and FINAL FANTASY XIV with Dalamud installed
* The game must have been run with Dalamud at least once

### Installing the Plugin

1. Launch the game with Dalamud enabled
2. Open the Dalamud Plugin Installer using `/xlplugins` in chat
3. Search for "RP Buddy" in the available plugins list
4. Click "Install" to add the plugin

## Usage

Once installed, RP Buddy works automatically in the background. Chat messages in supported channels will be recolored based on the following patterns:

- Text enclosed in `"quotes"` will be highlighted as dialogue
- Text enclosed in `*asterisks*` will be highlighted as actions/emotes

### Configuration

Access the plugin's configuration menu through:
- `/rpbuddy` command in chat, or
- The plugin's entry in the Dalamud Plugin Installer

## Development

### Building from Source

1. Clone this repository
2. Open `RpBuddy.sln` in [Visual Studio 2022](https://visualstudio.microsoft.com) or [JetBrains Rider](https://www.jetbrains.com/rider/)
3. Build the solution (Debug or Release)
4. The compiled plugin will be at `RpBuddy/bin/x64/Debug/RpBuddy.dll`

### Development Prerequisites

* .NET Core 8 SDK
* XIVLauncher with Dalamud installed in default directories
  * Custom Dalamud path can be set via `DALAMUD_HOME` environment variable

### Testing in Development

1. Build the plugin
2. Use `/xlsettings` in-game to open Dalamud Settings
3. Go to `Experimental` tab and add the path to `RpBuddy.dll` in Dev Plugin Locations
4. Use `/xlplugins` → `Dev Tools` → `Installed Dev Plugins` to enable RP Buddy
5. Test the plugin functionality

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## License

This project is licensed under the AGPL-3.0-or-later license. See [LICENSE.md](LICENSE.md) for details.
