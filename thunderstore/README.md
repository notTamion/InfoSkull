# InfoSkull
White Knuckle Mod that projects more info right into your skull

## Installation

### Thunderstore
The mod is available on Thunderstore where you can install via their app.

### Manual
As with any BepInEx Mod:
1. Install BepInEx
2. Drop the mod dll into the `BepInEx/plugins` folder
3. Done

## Customization
Most of the customization is available via the added `Adjust UI` pause button,
here you can move and/or format ui elements by dragging or clicking on them to edit.

Certain config options are only available by using the `is` command in the
ingame console which is accessible by pressing `Shift + ~` or if you don't have a
tilde key `Shift + Fn + Esc`.

### Formats
All ui elements you can set the format of can contain the following variables

[//]: # (VARIABLES_DESCRIPTIONS_START)
`{level}`: level name\
`{level_time}`: level time\
`{height}`: player height\
`{best_level_time}`: Best level time\
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
`{ascent}`: highest height reached in this run\
`{velocity}`: your velocity\
`{health}`: your health. LEADERBOARD ILLEGAL\
`{extra_jumps}`: extra jumps you have remaining. LEADERBOARD ILLEGAL

[//]: # (VARIABLES_DESCRIPTIONS_END)

Example format: `{left_stamina} | {clock} | {right_stamina}`

Some of these might not work in specific game modes
