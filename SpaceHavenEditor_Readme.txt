# Moragar's Space Haven Save Editor (Alpha 20)

**Version:** 1.0 (As of 2025-04-15)
**Author:** Moragar
**Compatibility:** Space Haven Alpha 20

## Overview

Take control of your Space Haven adventure! This editor allows you to modify various aspects of your save game file (`game`) for Space Haven. Whether you need a few extra credits, want to fine-tune your crew, manage inventory, or experiment with ship sizes, this tool provides a user-friendly interface to do so.

---

## !!! IMPORTANT DISCLAIMER !!!

**Use this tool AT YOUR OWN RISK!** Editing save files can potentially lead to unexpected bugs, corrupted saves, or unpredictable game behavior. The creator of this tool is **NOT RESPONSIBLE** for any damage caused to your save games or your game installation.

**ALWAYS create manual backups** of your save game folder before using this or any other save editor. While this tool includes an optional automatic backup feature, relying solely on it is not recommended as a primary safety measure.

---

## Features

### 1. Global Save Settings
Modify game-wide settings stored directly in the save file:
* **Player Credits:** Set the amount of credits (`ca`) available.
* **Player Prestige Points:** Adjust the prestige points (`playerPrestigePoints`) related to the main Exodus Fleet quest line.
* **Sandbox Mode:** Toggle Sandbox Mode (`sandbox`) on or off.
* *(Note: These global settings are updated in the editor's memory when you click the "Update Globals (Memory)" button. You must use `File -> Save` to write these changes to your save file.)*

### 2. Ship Management
Manage the ships present in your save:
* **Ship Selection:** Choose a ship from the dropdown list to view and edit its details.
* **Ship Info:** Displays the selected ship's internal Name (`sname`), Owner (e.g., "Player" or faction ID), and current Size (raw Sx, Sy values).
* **Update Ship Size:** Change the dimensions of the selected ship.
    * Input the desired Width and Height in **grid squares** (valid range: 1-8).
    * The editor calculates the corresponding Sx/Sy values (Squares * 28) that are saved to the file.
    * The calculated "Canvas Size" (buildable grid squares) is displayed for reference.
    * **Warning:** Making ships significantly larger than vanilla sizes (max recommended: 8x8 squares, Sx/Sy=224) can cause graphical issues, pathfinding problems, or conflicts with other objects in-game. Use cautiously!
    * *(Note: Ship size changes update memory immediately but require `File -> Save`.)*

### 3. Detailed Crew Editing
Perform extensive modifications on your crew members for the selected ship:
* **Crew Roster:** View all crew members on the selected ship.
* **Create New Crew:** Add new members using the "Create New Crew Member" button. This opens a dedicated window where you can:
    * Set the new member's name.
    * Define their starting Attributes, Skills (using grids and "Set All" buttons for speed), and Traits (add/remove from lists).
    * The editor automatically assigns a new unique Entity ID (`entId`) by incrementing the game's master ID counter.
* **Edit Existing Crew:** Select a crew member from the list to edit their details in the tabs on the right:
    * **Attributes:** Modify core attributes (e.g., Bravery, Zest, Intelligence, Perception) by double-clicking the 'Value' cell.
    * **Skills:** Modify skill levels (e.g., Mining, Botany, Piloting, Weapons) by double-clicking the 'Level' cell.
    * **Traits:** View current traits. Add new traits from a dropdown or remove existing traits by selecting them in the grid.
    * **Conditions:** View active conditions (injuries, buffs, debuffs). Select a condition and click "Delete Selected Condition" to remove it.
    * **Relationships:** View and edit the selected character's relationship values (Friendship, Attraction, Compatibility) towards other known characters. Edit values directly in the grid. Paging controls appear if the relationship list is long.
* **Quick Set Buttons:** Use the "Set All Attributes to 5" and "Set All Skills to 8" buttons within the respective tabs to quickly apply these values to the selected character
* *(Note: Adding crew, editing grids (Attributes, Skills, Relationships), and managing traits/conditions require `File -> Save` to be permanent. The "Set All..." buttons update memory immediately but still need a `File -> Save`.)*

### 4. Storage Management
Easily manage the contents of storage containers on the selected ship:
* **Container Selection:** Choose a storage container (standard storage, lockers, specific production facilities that hold items) from the dropdown. The editor identifies these based on specific XML attributes (`eatAllowed="true"`).
* **View Contents:** Displays items in the selected container with name, quantity, and internal ID. The total quantity of all items in the selected container is also shown.
* **Edit Quantity:** Modify the number of items in a stack by double-clicking the 'Quantity' cell. Setting the quantity to 0 will remove that item stack when the game is saved.
* **Add Items:** Select an item from the categorized dropdown list, enter a quantity, and click "Add to Container". Adds to existing stacks or creates a new one.
* **Delete Item Stack:** Select an item row in the grid and click "Delete Selected" to remove the entire stack.
* *(Note: All storage modifications require `File -> Save`.)*

### 5. Automatic Backups (Optional)
* **Configuration:** Enable or disable via the "Edit" -> "Settings" menu. The setting is saved and remembered between sessions.
* **Functionality:** When enabled, opening a save file (`File -> Open`) automatically creates a timestamped copy of the *entire folder* containing the save (e.g., if you open `...\savegames\MySave\save\game`, it backs up the `...\savegames\MySave\save` folder) into the parent `savegames` directory (e.g., `...\savegames\MySave_backup_YYYYMMDDHHMMSS`).
* **Purpose:** Provides a safety net against accidental corruption, but manual backups are still strongly advised.

---

## Requirements

* .NET Desktop Runtime 7.x (or newer, if the editor is updated). You can download it from Microsoft if you don't have it.
* Windows 10/11

---

## Installation

1.  Download the editor archive (.zip).
2.  Extract the contents to a folder anywhere on your computer (e.g., `C:\Tools\SpaceHavenEditor`).
3.  Run the `SpaceHavenEditor.exe` file.

---

## How to Use

1.  **Launch Editor:** Run `SpaceHavenEditor2.exe`.
2.  **Open Save File:**
    * Go to `File -> Open`.
    * Navigate to your Space Haven save game directory. The typical path is:
        `Steam\steamapps\common\SpaceHaven\savegames\[YourSaveGameName]\save\`
    * Select the file named `game` (it usually has no file extension).
    * Click "Open". Wait for the "Save loaded" confirmation.
    * (A backup might be created automatically in the `savegames` folder if enabled in Settings).
3.  **Select Ship:** Use the "Selected Ship" dropdown to choose the ship you want to edit.
4.  **Edit Data:**
    * Use the "Global Save Settings" section for credits, prestige, and sandbox mode. Click "Update Globals (Memory)" after changing these.
    * Use the "Crew" or "Storage" tabs for ship-specific edits.
    * Use the inner tabs (Attributes, Skills, etc.) within the Crew tab for character details.
    * Make edits by double-clicking grid cells, using dropdowns, or clicking Add/Delete buttons as described in the Features section.
5.  **Save Changes:**
    * Go to `File -> Save`.
    * This writes **all** changes currently held in memory (from global updates, crew edits, storage edits, ship size changes) to your actual `game` file.
6.  **Load in Game:** Close the editor and launch Space Haven. Load the modified save game to see your changes.

---

## Known Issues / Limitations

* Currently assumes specific XML structures found in Alpha 20 saves. Future game updates might break compatibility.
* Editing relationships requires understanding character IDs, although names are displayed for convenience.
* Does not edit all possible save game values (e.g., ship layout, research progress, faction relations). - Working on expanding this later

---

## Troubleshooting

* **Editor Crashes on Open/Save:** Ensure you have the correct .NET Desktop Runtime installed. Verify the save file is not corrupted. Try disabling automatic backups.
* **Changes Not Appearing In-Game:** Make sure you clicked `File -> Save` in the editor after making your changes. Ensure you are loading the correct save file in Space Haven.
* **XML Errors:** If you encounter errors related to XML structure (e.g., "node not found"), the save file might be corrupted, from an incompatible game version, or modified by another tool. Restore from a backup.

---

## Credits

* **Author:** Moragar
---

Enjoy customizing your Space Haven experience!
