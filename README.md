# RP Buddy

A Dalamud plugin designed to enhance the roleplaying experience by providing visual tools for chat customization.

## Features

### RP Icon in Chat
For ease of access to see who around you is roleplaying, RP Buddy shows the RP Icon next to player names in chat if they are in the Roleplaying status.

### Treat Say as an Emote Chat
This turns the /say Chat into the same thing as if someone would type their message as custom emote.

### Chat Recoloring
RP Buddy automatically recolors chat messages based on roleplaying conventions:
- **Quoted text** (text within quotation marks `"..."`) - Recolored to distinguish spoken dialogue (Uses the say log color)
- **Emoted actions** (text within asterisks `*...*` or less than/greater than `<...>`) - Recolored to distinguish character actions and emotes (Uses the emote log color)
- **OOC Text** (text within `[...]`, `[[...]]`, `(...)`, `((...))`) - Recolored to distinguish out of character stuff (Uses the echo log color)

### "Improved" Indicators for continued/done markers
You are part of those who like to write *very* long roleplay? Then this might be (or not) for you.
This one is easier shown than written down.

![preview of improved indicators](https://github.com/Syrilai/rp-buddy/blob/master/img/030543_QUKbJsT0MF.png?raw=true)

### Vertical Lines for changes in the scenery or such
Can be used for anything, really. When you start your message (or emote) with a `|`, then it moves the entire message onto a new line.

---
All of these things can be seen in the in-game introduction as well, more visually. Or you try them out yourself, no one is going to stop you.

## Installation

### Prerequisites

* XIVLauncher and FINAL FANTASY XIV with Dalamud installed
* The game must have been run with Dalamud at least once

### Installing the Plugin

1. Launch the game with Dalamud enabled
2. Open your Dalamud Settings using `/xlsettings`
3. Add the following URL as a custom plugin repository: `https://raw.githubusercontent.com/Syrilai/rp-buddy-repo/main/pluginmaster.json` and save
4. Open the Dalamud Plugin Installer using `/xlplugins` in chat
5. Search for "RP Buddy" in the available plugins list
6. Click "Install" to add the plugin
