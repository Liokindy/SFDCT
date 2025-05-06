### IMPORTANT

> [!WARNING]
> SFDCT releases will ***only*** work in the SFD version specified for that release, any other version of SFD will ***not*** work

> [!NOTE]
> SFDCT is currently being tested and ported to SFD `1.4.1b` (I'm not experienced with Harmony, most of the technical side was previously done by SFR)

> [!NOTE]
> This README is currently incomplete. Current [releases](https://github.com/Liokindy/SFDCT/releases) target SFD `1.3.7d`, so they will ***only*** work with that SFD version. It can be downloaded by choosing the `sfd_v_1_3_7d - SFD v1.3.7d` beta on Steam.

<details>
<summary>SFDCT</summary>

SFDCT is a mod for [Superfighters Deluxe](https://store.steampowered.com/app/855860/Superfighters_Deluxe), it adds or tweaks small features while mantaining compatibility with vanilla-SFD.

Meaning that a player can join a normal SFD server with SFDCT, and players using normal SFD can join a server being hosted with SFDCT.

> [!NOTE]
> This mod is in development. Features may be added, removed, or changed. Or the mod may break with SFD's updates.

</details>

<details>
<summary>Installation</summary>

> [!WARNING]
> SFDCT may get flagged as malicious by your OS or anti-virus. This is a known problem. I can only advice you to ***NOT*** trust downloads of SFDCT from sources that are not from the official repository (https://github.com/Liokindy/SFDCT). If you have doubts, you can freely revise the code and build SFDCT yourself.

1. Download a release (https://github.com/Liokindy/SFDCT/releases) and extract the contents to SFD's folder. Commonly located at `C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe`

2. Go to SFD's launch options in Steam and copy-paste these launch options: `cmd /c "%command%\..\SFDCT.exe"`

3. When you play SFD, a console will pop up and SFDCT will start patching SFD.

*(The text may display different stuff on your release)*

</details>

<details>
<summary>Credits</summary>

Superfighters Deluxe was created and is owned by [Mythologic Interactive](https://mythologicinteractive.com/SuperfightersDeluxe)

SFDCT started by using an old version of [Superfighters Redux](https://github.com/Odex64/SFR) with its custom assets and features removed.

Many ideas were suggested to me by my friends (xoxo `ElDou's1`).

</details>

<details>
<summary>Building</summary>

### Prerequisites
- [Visual Studio](https://visualstudio.microsoft.com/) with ".NET Desktop development" and .NET Framework 4.7.2 SDK installed
- [dnSpy](https://github.com/dnSpyEx/dnSpy)

Clone or download SFDCT's source code and open its solution with Visual Studio. Wait for NuGet to install all dependencies.

Right click on SFDCT's project and choose properties, change your configuration from `Active` to `All Configurations`

In the `Debug` section change your working directory to your SFD installation and external program to `SFDCT.exe`

> [!NOTE]
> If you don't have a `SFDCT.exe` to select, create a dummy file (You may do this by duplicating `Superfighters Deluxe.exe` and renaming it) and choose that

If you have installed SFD in a another directory or drive, you must modify `build.bat` as well. You need to change `SFD` variable with your actual installation path.

One last step is to create a `SFDCT` folder inside your Superfighters Deluxe installation, and manually copy `Core.dll` and `Content` folder from SFDCT solution to the newly created folder. Now in Visual Studio try to build the solution, if you don't see any errors you're good to go!

You can open `Core.dll` with dnSpy in order to inspect SFD code. It is a slightly modified `Superfighters Deluxe.exe` assembly.
</details>