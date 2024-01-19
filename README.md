# cs2-surf-cfg-dumper

## Introduction
This project is designed for CS2 server owners, mappers and plugin developers. The tool analyzes workshop maps downloaded on your computer, focusing on identifying `<mapname>.cfg` files and any `point_servercommand` and `logic_autos` within them.

## Background
In Source2 engine for CS2, there are two methods that maps may executing commands on a server:

1. **Map CFG**: Executed on map load.
2. **point_servercommand and logic_auto**: Typically executed on round start.

Unfortunately, these execution methods pose challenges for server owners, as there's no native way to disable them. This often leads to issues, especially when maps set `sv_cheats` or alter movement configurations, which can disrupt the surf game mode.

Plugins must be defensive and override or set back to default any weird crap executed by maps.

## Goal
The goal of this repository is to store configurations of surf maps in an easily accessible manner. This assists in identifying and potentially overriding undesired configurations set by the maps and showing mappers what is accepted CFG's. The output of the tool is stored in the **/output** directory and the source code of the tool is in **/src**

## Example of good config
- [surf_beginner](https://github.com/ws-cs2/cs2-surf-cfg-dumper/blob/main/output/surf_beginner.txt)

## Example of bad configs

- [surf_atrium](https://github.com/ws-cs2/cs2-surf-cfg-dumper/blob/main/output/surf_atrium.txt) - Sets sv_cheats 1 and sv_gravity to 850
- [surf_gleam](https://github.com/ws-cs2/cs2-surf-cfg-dumper/blob/main/output/surf_gleam.txt) - Sets sv_maxvelocity to 99999 when the map was made for 3500

## Config template
If you are a mapper and unsure what to set in your cfg, copy this:

```
sv_gravity 800
sv_airaccelerate 150
sv_enablebunnyhopping 1
sv_autobunnyhopping 1
sv_falldamage_scale 0
sv_staminajumpcost 0
sv_staminalandcost 0
sv_disable_radar 1

sv_maxvelocity 3500 // Most surf maps are played with 3500 max velocity, some maps can be played with upto 4096 in CS2, this is a choice a mapper should make.

mp_roundtime 60
mp_round_restart_delay 0
mp_freezetime 0
mp_team_intro_time 0
mp_drop_knife_enable 1
mp_warmup_end
mp_warmuptime 0
bot_quota 0

exec maps/<mapname>_server.cfg
```

## Limitations
- While it's possible to remove `point_servercommand` entities to prevent their execution, currently, there's no known method to stop the execution of configurations embedded in the CFG section of maps.

