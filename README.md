My own mod for [Superfighters Deluxe](https://store.steampowered.com/app/855860/Superfighters_Deluxe), using [Superfighters Redux](https://github.com/Odex64/SFR) as base. It tweaks and adds some features I personally like to have in SFD.

<img src="docs/SFD_titleLoop.gif" alt="SFD"/>

# Notable Features
> [!NOTE]
> The mod is still a work-in-progress. Features may break, be added, removed, or changed entirely.

- Sound panning and distortion. Sounds will pan to the left or right depending on where they come from, and they will sound slowed down or speed up according to the time modifier. Chaos is now even more cinematic during slowmotions.
- Use more than 8 slots for players/bots in your server. (beware this may break Maps and/or Scripts)
- Cycle through your messages in chat using the UP and DOWN arrow keys (like Minecraft!)
- Choose your own custom color for the UI.

# How to install
> [!WARNING]
> SFDCT may get flagged as malicious by your browser and/or OS. This is a **known** problem, due to this, if SFDCT gets flagged on your end, you'll have to create an exception for it. We're sorry for this issue.

1. Get the latest release [here](https://github.com/Liokindy/SFDCT/releases). Go to SFD in Steam, right-click it, go to "Manage", and then "Browse local files". Extract the 
zip contents in there.

2. Next, go back to Steam and right-click SFD, then "Properties...". Go to launch options, and type `/c "%command%\..\SFDCT.exe"`. This will tell Steam to open SFDCT in a cmd instead of SFD directly.

3. When you open SFD, you'll get asked to open SFDCT, vanilla-SFD, or even SFR if you have a release already installed. If you'd like to start a specific game, skipping the selection, you can do so using a start parameter `-SFDCT`, `-SFD`, or `-SFR`.

# License
> The **Superfighters Redux** mod is allowed to **adapt and modify Superfighters Deluxe** and it's content to allow for **other mod integration that is not Superfighters Redux.**

SFDCT is a fork of [SFR](https://github.com/Odex64/SFR). You can read the [SFR license file](https://github.com/Odex64/SFR/blob/master/LICENSE.txt) for further clarification and details.

# How to Build
You can follow [SFR's building guide](https://github.com/Odex64/SFR/blob/master/CONTRIBUTE.md), as the process is quite similar, if not the same for both.
