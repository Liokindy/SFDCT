# Contents

1. [Building](CONTRIBUTING.md#building)
2. [Localization](CONTRIBUTING.md#localization)

## Building

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

## Localization

SFDCT uses the same system SFD uses for localization, that is, text is stored in an XML file and later accessed and formated with arguments in-game.

Here's a snippet of SFDCT's `SFDCT_default.xml` language file:

```xml
<Texts name="Default">
    <!--  ...  -->
    <Text id="sfdct.command.servermouse.message">Server-Mouse set to {0}</Text>
    <Text id="sfdct.command.servermousemoderators.message">Server-Mouse moderators set to {0}</Text>
    <Text id="sfdct.command.addmodcommands.header">Adding moderator commands...</Text>
    <!--  ...  -->
</Texts>
```

Inside there are `<Text>` elements with an `id` that define texts. In these texts there are arguments that are later replaced with other text in-game, such as `{0}`, `{1}`, etc.

To create a new language file copy the default one and rename it to something else. Like `SFDCT_mynewlanguage.xml`. Now you can freely edit or translate the contents shifting the arguments about depending on the text as some languages may not have the same "syntax" as English.

Here's a snipper of a modified language file.
```xml
<Texts name="CustomDefault">
    <!--  ...  -->
    <Text id="sfdct.command.servermouse.message">the sv mouse has been set to {0}</Text>
    <Text id="sfdct.command.servermousemoderators.message">no sv mouse mods is {0} from now on</Text>
    <Text id="sfdct.command.addmodcommands.header">creating moderator wizard spells...</Text>
    <!--  ...  -->
</Texts>
```

Inside SFDCT settings panel, or in the `config.ini` file there's a Language setting that holds the name of the language file to be used, by default it is set to `SFDCT_default`. Set it to the name of your file without the `.xml` extension and restart SFDCT. If the file is not found an error message will appear on SFDCT's console and the language will be reverted to `SFDCT_default`.
