# cs2-surf-cfg-dumper

## Introduction
This project is designed for CS2 server owners, mappers and plugin developers. The tool analyzes workshop maps downloaded on your computer, focusing on identifying `<mapname>.cfg` files and any `point_serverentities` and `logic_autos` within them.

## Background
In Source2 engine for CS2, there are two methods that maps may executing commands on a server:

1. **Map CFG**: Executed on map load.
2. **Point_ServerCommand and Logic_Auto**: Typically executed on round start.

Unfortunately, these execution methods pose challenges for server owners, as there's no native way to disable them. This often leads to issues, especially when maps set `sv_cheats` or alter movement configurations, which can disrupt the surf game mode.

Plugins must be defensive and override or set back to default any weird crap executed by maps.

## Goal
The goal of this repository is to store configurations of surf maps in an easily accessible manner. This assists in identifying and potentially overriding undesired configurations set by the maps and showing mappers what is accepted CFG's. The output of the tool is stored in the **/output** directory and the source code of the tool is in **/src**

## Example of bad configs

- [surf_atrium](https://github.com/ws-cs2/cs2-surf-cfg-dumper/blob/main/output/surf_atrium.txt) - Sets sv_cheats 1 and sv_gravity to 850
- [surf_gleam](https://github.com/ws-cs2/cs2-surf-cfg-dumper/blob/main/output/surf_gleam.txt) - Sets sv_maxvelocity to 99999 when the map was made for 3500

## Limitations
- While it's possible to remove `point_servercommand` entities to prevent their execution, currently, there's no known method to stop the execution of configurations embedded in the CFG section of maps.

