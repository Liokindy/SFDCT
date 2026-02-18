LIOKINDI DAME TETA
> [!TIP]
> If you don't have a `SFDCT.exe` to select, create a dummy file and choose that. It will get replaced when you build the solution.

If you have installed SFD in a another directory or drive, you must modify `build.bat` as well. You need to change `SFD` variable with your actual installation path.

One last step is to create a `SFDCT` folder inside your Superfighters Deluxe installation, and manually copy `Core.dll` and `Content` folder from SFDCT solution to the newly created folder.

Now in Visual Studio try to build the solution, if you don't see any errors you're good to go!

You can open `Core.dll` with dnSpy in order to inspect SFD code. It is a slightly modified `Superfighters Deluxe.exe` assembly.

> [!TIP]
> You can learn how to write patches using this [Harmony guide](https://harmony.pardeike.net/articles/patching.html)
