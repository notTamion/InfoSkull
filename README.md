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

[//]: # (VARIABLES_DESCRIPTIONS_START)
`{level}`: level name\
`{level_time}`: level time\
`{height}`: player height\
`{best_level_time}`: Best level time, requires level timer saving to be enabled\
`{ascent_rate}`: ascent rate\
`{game_time}`: time in seconds since start of run\
`{clock}`: clock displaying the time\
`{left_stamina}`: stamina of your left Hand. LEADERBOARD ILLEGAL\
`{right_stamina}`: stamina of your right Hand. LEADERBOARD ILLEGAL\
`{mass_height}`: mass height. LEADERBOARD ILLEGAL\
`{mass_speed}`: mass speed. LEADERBOARD ILLEGAL\
`{mass_acc_mult}`: mass acceleration multiplier\
`{mass_distance}`: distance from mass to player. LEADERBOARD ILLEGAL\
`{score}`: score \
`{high_score}`: high score\
`{ascent}`: highest height reached in this run

[//]: # (VARIABLES_DESCRIPTIONS_END)

Example: `is display format {left_stamina} | {clock} | {right_stamina}`

Some of these might not work in specific game modes
