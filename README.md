# DungeonOfTheEndless-Mod
This is a compilation of some mods that I use for DotE.

# Tutorial
There are a few steps that you must take in order to properly use these mods.
## Important Downloads
1. Please download [Partiality](https://github.com/PartialityModding/PartialityLauncher/releases/download/0.3.11/PartialityLauncher.zip)
2. Please also download the following [MonoMod](https://0x0ade.visualstudio.com/e07cb5e7-fa7f-457d-982b-3323979ed1b7/_apis/build/builds/156/artifacts?artifactName=net35&api-version=5.0-preview.4&%24format=zip)
3. Download the [DustDevilFramework.dll](https://github.com/sc2ad/DungeonOfTheEndless-Mod/releases/download/2.0.0/DustDevilFramework.dll)
3. Download any number of the DLLs that have `_Mod` in their name from [The releases page](https://github.com/sc2ad/DungeonOfTheEndless-Mod/releases)
## Setup
**PLEASE FOLLOW THIS AS BEST AS YOU CAN, IF YOU DO NOT, THE MODS WON'T LOAD AND YOU MAY HAVE TO REINSTALL YOUR GAME!**
1. Extract Partiality to a safe place (this is how you will load the mods into the game, don't lose it!)
2. Run the `Partiality Launcher.bat` file.
3. Navigate to `File -> Open Game` (or press Ctrl-O)
4. Navigate to your `DungeonoftheEndless.exe`. This is normally located at: `C:\Program Files (x86)\Steam\steamapps\common\Dungeon of the Endless`
5. Enter the `APPID` of Dungeon of the Endless, which is: `249050`
6. Click the `Apply Mods` Button (even though there are no mods!)
7. You may load your game as normal, and confirm there are no problems.
8. Next, open the `net35` folder that you should have gotten when you downloaded [the MonoMod link above](https://0x0ade.visualstudio.com/e07cb5e7-fa7f-457d-982b-3323979ed1b7/_apis/build/builds/156/artifacts?artifactName=net35&api-version=5.0-preview.4&%24format=zip)
9. Copy all of these files, and paste them into the `DungeonoftheEndless_Data\Managed` directory of your Dungeon of the Endless exe. This is normally located at: `C:\Program Files (x86)\Steam\steamapps\common\Dungeon of the Endless\DungeonoftheEndless_Data\Managed`. If prompted to replace files, do so.
- **Also paste these files into the `bin` directory in the same directory as `Partiality Launcher.bat`. If this is _not_ done, then you will only be able to patch mods _one_ time!**
10. Copy the `DustDevilFramework.dll` and place it into the `Managed` location from above. If you are prompted to replace a file, you have done something wrong, you should reinstall your game and try again.
11. Copy any number of the mods that you have downloaded [from the releases page](https://github.com/sc2ad/DungeonOfTheEndless-Mod/releases) and place them into the `Mods` folder of the `DungeonoftheEndless.exe`. This will be located at wherever your `DungeonoftheEndless.exe` is, which is normally: `C:\Program Files (x86)\Steam\steamapps\common\Dungeon of the Endless\Mods`
- **Please remember that you should only be copying DLL files, and not anything else!**
12. Go back to `Partiality`, which you should reopen, and click the `Refresh Mod List` option.
13. Check off as many mods as you desire, and press `Apply Mods` and enjoy!
- Please note that you can change various configuration of these mods in their respective config files, which are generated (by default) in the directory with `DungeonoftheEndless.exe` after loading them for the first time.
