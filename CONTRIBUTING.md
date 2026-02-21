# CONTRIBUTING

## Language

- [![English](https://img.shields.io/badge/Language-English-blue)](CONTRIBUTING.md)
- [![Español](https://img.shields.io/badge/Idioma-Español-red)](CONTRIBUTING.es.md)

## Building

You will need:
- Experience with C# and Reflection.
- [Visual Studio](https://visualstudio.microsoft.com/) with ".NET Desktop development" and .NET Framework 4.7.2 SDK installed
- [dnSpy](https://github.com/dnSpyEx/dnSpy)

### 1. GET THE REPOSITORY'S FILES

You can manually download it in "code" -> "download ZIP", or you can clone it using Git or GitHub Desktop.

<p><img src="./docs/png/building_0.png" alt="Getting the files from the repository"/></p>

### 2. SETUP THE SOLUTION

- Open the solution file (`.sln`) with Visual Studio.
- Wait for NuGet to install all dependencies.
- Right click on `SFDCT` and choose properties.

<p><img src="./docs/png/building_1.png" alt="Visual Studio Solution Properties"/></p>

- Go to the "debug" tab on the side and change your configuration to "all configurations".
- Go to your Superfighters Deluxe installation, copy and paste `Superfighters Deluxe.exe` to create a dummy file, rename it to `SFDCT.exe`.
- On "start action" change to "start external program", navigate to the dummy file you created and select it, i.e: `C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe\SFDCT.exe`.
- On "start options" Change the working directory to your Superfighters Deluxe installation root folder, i.e: `C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe\`.

<p><img src="./docs/png/building_2.png" alt="Visual Studio Solution Properties"/></p>

> [!NOTE]
>
> If you have installed Superfighters Deluxe in a another directory or drive, you must modify `build.bat`.
> - Change `SFD` variable with yours, i.e: `SET SFD="C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe"`
> 

### 3. SETUP SFDCT

- Go to your Superfighters Deluxe installation and create a `SFDCT` folder.
- Go back to the repository's files and copy these to the folder you created:
- - `Content` folder
- - `Core.dll` file inside `SFD`.

<p><img src="./docs/png/building_3.png" alt="Visual Studio Solution Properties"/></p>

### 4. BUILDING AND TESTING

In Visual Studio try to build the solution, you can use the "build solution" button at "build" on the top bar or use `Ctrl + Shift + B`. If you don't see any errors and see messages of files being copied in the output: You did it, nice!

Open `Core.dll` with dnSpy in to inspect SFD's code, the main way to modify behavior is through Harmony patches. You can learn more [about patching using this official Harmony guide](https://harmony.pardeike.net/articles/patching.html), you can learn more about [harmony's transpiler patches with this Terraria guide](https://gist.github.com/JavidPack/454477b67db8b017cb101371a8c49a1c).

> [!TIP]
>
> - There are some SFD settings you can change make the initial loading faster like resolution or disabling the music.
> - You can use Visual Studio's debugger by attaching it to the `SFDCT.exe` process.
> 

## Localization

SFDCT uses the same system SFD uses for languages: language-specific texts are stored in a file, referenced with IDs and formated with arguments given by the game.

Here is a snippet of SFDCT's `SFDCT_default.xml` language file:

```xml
<Texts name="Default">
    <!--  ...  -->
    <Text id="sfdct.command.servermouse.message">Server-Mouse set to {0}</Text>
    <Text id="sfdct.command.servermousemoderators.message">Server-Mouse moderators set to {0}</Text>
    <Text id="sfdct.command.addmodcommands.header">Adding moderator commands...</Text>
    <!--  ...  -->
</Texts>
```

In the language-specific texts there are arguments (`{0}`, `{1}`, `{2}`, etc.) that are later replaced by the game.

To create a new language for SFDCT:
- Copy the default one and rename it, **keeping `SFDCT_` at the start**, i.e: `SFDCT_mynewlanguage.xml`.
- Edit/Translate the language-specific texts, **keeping in mind the arguments and their order**.

Inside SFD go to SFDCT's settings (or in the `config.ini` file) and check inside the "language" dropdown if there's an option that holds the name of your language file, you may need to restart SFD to refresh this list. If the file is not found an error message will appear on SFDCT's console and the language will be reverted to `SFDCT_default`.
