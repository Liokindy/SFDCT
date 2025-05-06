> [!IMPORTANT]
> SFDCT releases will ***only*** work in the SFD version specified for that release, any other version of SFD will ***not*** work

> [!NOTE]
> SFDCT is currently being tested and ported to SFD `1.4.1b` (I'm not experienced with Harmony, most of the technical side was previously done by SFR)

> [!WARNING]
> This README is currently incomplete. Current [releases](https://github.com/Liokindy/SFDCT/releases) target SFD `1.3.7d`, so they will ***only*** work with that SFD version. It can be downloaded by choosing the `sfd_v_1_3_7d - SFD v1.3.7d` beta on Steam.

## SFDCT

> [!NOTE]
> This mod is in development. Features may be added, removed, or changed. Or the mod may break with SFD's updates.

SFDCT is a mod for [Superfighters Deluxe](https://store.steampowered.com/app/855860/Superfighters_Deluxe), it adds or tweaks small features while mantaining compatibility with vanilla-SFD.

Meaning that a player can join a normal SFD server with SFDCT, and players using normal SFD can join a server being hosted with SFDCT.


## INSTALLATION

> [!CAUTION]
> SFDCT may get detected as a *malicious program*. This is a known problem.
>
> Do ***NOT*** download SFDCT from sources that are not [***THIS***](https://github.com/Liokindy/SFDCT/releases) official repository's releases.
>
> If you have doubts, you can manually review the source code and build SFDCT on your own PC.

1. **DOWNLOAD A RELEASE**

Extract the contents to Superfighters Deluxe's root folder.

> [!TIP]
> SFD is usually located at `C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe`.
>
> You can also go to Steam -> `Manage` -> `Browse local files`

2. **LAUNCH OPTIONS**

Inside Steam, go to Superfighters Deluxe's `Properties...` -> `LAUNCH OPTIONS` and copy these launch options:
    
    cmd /c "%command%\..\SFDCT.exe"

3. **OPEN SFD**

When you open Superfighters Deluxe, a console will open, inside you should see colored text. *(The text may vary on your release)*

    [12:00:00] Starting SFDCT...
    [12:00:00] Patching completed...
    Setting breakpad minidump AppID = 855860
    Steam_SetMinidumpSteamID:  Caching Steam ID:  1234567890 [API loaded no]

## CREDITS

Superfighters Deluxe was created and is owned by [Mythologic Interactive](https://mythologicinteractive.com/SuperfightersDeluxe)

SFDCT started by using an old version of [Superfighters Redux](https://github.com/Odex64/SFR) with its custom assets and features removed.

Many ideas were suggested to me by my friends (xoxo `ElDou's1`).


## BUILDING

### PREREQUISITES
- [Visual Studio](https://visualstudio.microsoft.com/) with ".NET Desktop development" and .NET Framework 4.7.2 SDK installed
- [dnSpy](https://github.com/dnSpyEx/dnSpy)

Clone the repository, or download the Source code.

Open its solution with Visual Studio. Wait for NuGet to install all dependencies. Right click on SFDCT's project and choose properties, change your configuration from `Active` to `All Configurations`

In the `Debug` section change your working directory to your SFD installation and external program to `SFDCT.exe`

> [!TIP]
> If you don't have a `SFDCT.exe` to select, create a dummy file and choose that. It will get replaced when you build the solution.

If you have installed SFD in a another directory or drive, you must modify `build.bat` as well. You need to change `SFD` variable with your actual installation path.

One last step is to create a `SFDCT` folder inside your Superfighters Deluxe installation, and manually copy `Core.dll` and `Content` folder from SFDCT solution to the newly created folder.

 Now in Visual Studio try to build the solution, if you don't see any errors you're good to go!

> [!IMPORTANT]
> You can open `Core.dll` with dnSpy in order to inspect SFD code. It is a slightly modified `Superfighters Deluxe.exe` assembly.
> 
> You can write patches using [this Harmony guide](https://harmony.pardeike.net/articles/patching.html).