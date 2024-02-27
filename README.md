My own mod for [Superfighters Deluxe](https://store.steampowered.com/app/855860/Superfighters_Deluxe). 
- It adds some features I personally like to have in vanilla SFD. 
- Players without SFDCT can join you server, and you can join vanilla servers with SFDCT.

<img src="docs/SFD_titleLoop.gif" alt="SFD"/>

# Notable Features
> [!NOTE]
> The mod is still a work-in-progress. Features may break, be added, removed, or changed entirely.

> [!TIP]
> Settings are currently accessed and changed by editing `SFDCT/config.ini` with a text editor. You can refresh your settings in-game using `F6`

### Sound Panning
- Sounds will pan to the left or right depending on where they come from
- Sound will distort and sound slowed down or speed up according to the time modifier.

### More profiles
- You have twice the amount of profiles to use, from 9 to a whopping 18.

<img src="docs/extendedProfiles.gif" alt="ExtendedProfiles"/>

### More slots
> [!WARNING]
> This is feature is still in an experimental and WIP state. It may break maps and scripts that assume there's only 8 slots available, or there may be other yet-unknown issues.

> [!TIP]
> Other players will see the scoreboard glitched, and the `/PLAYERS` command will not work. A server-side replacement is in place for players to see others score and team, `/SCOREBOARD`

- Expanded scoreboard to show more than 8 players, after 16 slots it may look glitched in some resolutions like 720p.
- Set more than 8 slots for players or bots to use in your server.
- Activated with the `-SLOTS [8-32]` start parameter.

<img src="docs/10SlotsScoreboard.png" alt="10SlotsScoreboard"/>
<img src="docs/20slotsChaos.gif" alt="20SlotsChaos"/>

### QoL and Customization
- Cycle through your messages in chat using the `UP` and `DOWN` arrow keys (like Minecraft!)

<img src="docs/chatCycling.gif" alt="ChatCycling"/>

- Choose your own custom color for the UI.

<img src="docs/customUI0.png" alt="CustomUI0"/>
<img src="docs/customUI1.png" alt="CustomUI1"/>

# Installation
> [!WARNING]
> SFDCT may get flagged as malicious by your browser/OS. This is a **known problem**, however, due to this we advice you to **not** trust downloads of SFDCT's releases from sites outside the [Official Repository](https://github.com/Liokindy/SFDCT/).

1. Get the latest release [here](https://github.com/Liokindy/SFDCT/releases). Go to SFD's root folder and extract the zip contents there.

2. Go to SFD's launch options in Steam and type `cmd /c "%command%\..\SFDCT.exe"`. This will tell Steam to open SFDCT instead of SFD.

3. When you launch SFD, a cmd will appear, and SFDCT will boot instead.

# Credits
SFDCT is made using [SFR](https://github.com/Odex64/SFR) as a base, however, assets, features, etc. From SFR are not included.

# Building
You can use [SFR's](https://github.com/Odex64/SFR) building guide, as the process is similar.