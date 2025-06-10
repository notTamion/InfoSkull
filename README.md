# InfoSkull
White Knuckle Mod that projects more info right into your skull

## Installation
As with any BepInEx Mod:
1. Install BepInEx
2. Drop the mod dll into the `BepInEx/plugins` folder
3. Customize
4. Done

## Customization
You can customize the mod in the ingame console which you can open by pressing `Shift + ~` 
or `Shift + Fn + Esc` if you're on a keyboard that doesn't have a tilde key.

Then in the console you can use the `is` command and follow the subcommands.

You can also customize the mod directly in the config file which is located in the `BepInEx/config` directory

## Formats
Anything set via the `format` subcommands can have the following variables

`{level}`: Current level name\
`{level_time}`: Current level time\
`{best_level_time}`: Best level time, requires level timer saving to be enabled\
`{ascent_rate}`: Current ascent rate\
`{game_time}`: Current time in seconds since start of game\
`{clock}`: A clock displaying the time\
`{left_stamina}`: Stamina of your left Hand\
`{right_stamina}`: Stamina of your right Hand

Example: `is display format {left_stamina} | {clock} | {right_stamina}`

Some of these might not work in specific game modes
