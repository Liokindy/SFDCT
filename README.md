<p align="center"><img src="./docs/gif/sfdct_title_loop.gif" alt="SFDCT Logo"/></p>

> [!NOTE]
> This mod is in development. Features may be added, removed, or changed.

Superfighters Custom is a mod for [Superfighters Deluxe](https://store.steampowered.com/app/855860/Superfighters_Deluxe). It adds and tweaks some features while mantaining compatibility with vanilla-SFD. Meaning that a player can join a normal SFD server with SFDCT, and players using normal SFD can join a server being hosted with SFDCT.

<details>

<summary>Installation</summary>
<br>

> [!CAUTION]
> SFDCT may get detected as a *malicious program*. This is a known problem.
>
> Do ***NOT*** download SFDCT from sources that are not [***THIS***](https://github.com/Liokindy/SFDCT/releases) official repository's releases.
>
> If you have doubts, you can manually review the source code and build SFDCT on your own PC.

1. **DOWNLOAD A RELEASE**

> [!IMPORTANT]
> SFDCT releases will ***only*** work in the SFD version specified for that release, any other version of SFD will ***not*** work

Extract the contents to Superfighters Deluxe's root folder.

2. **LAUNCH OPTIONS**

Inside Steam, go to Superfighters Deluxe's `Properties...` -> `LAUNCH OPTIONS` and copy these launch options:

<p><img src="./docs/png/installation_0.png" alt="Steam Launch Options"/></p>
    
    cmd /c "%command%\..\SFDCT.exe"

3. **OPEN SFD**

When you open Superfighters Deluxe, a console will open, inside you should see colored text.

<p><img src="./docs/png/installation_1.png" alt="SFDCT Console"/></p>

</details>

<details>

<summary>Credits</summary>

#### SFDCT

- Azure (Ideas)
- ElDou's1 (Ideas, Tester)
- Liokindy (Developer)
- Nult (Ideas)

#### SPECIAL THANKS
- Developers of [Superfighters Redux](https://github.com/Odex64/SFR)
- Original developers of [Superfighters Deluxe](https://mythologicinteractive.com/SuperfightersDeluxe)

</details>

<details>
<summary>Building</summary>

#### PREREQUISITES
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

You can open `Core.dll` with dnSpy in order to inspect SFD code. It is a slightly modified `Superfighters Deluxe.exe` assembly.

> [!TIP]
> You can learn how to write patches using this [Harmony guide](https://harmony.pardeike.net/articles/patching.html)
</details>