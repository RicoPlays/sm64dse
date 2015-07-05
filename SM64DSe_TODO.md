# TODO #

There's no particular priority order unless stated otherwise.

## Linux ##

  * fix the issues with GLControl (those little bastards overlap everything and ruin the active form's rendering, like [this](http://i.imgur.com/afL9E.png))
  * make a Linux build with Monodevelop (will perhaps fix the aforementioned issue)

## Model importer ##

  * remove the texture scaling trick (it only works on emulators)
  * add user-friendly warnings when potential problems are detected (like the aforementioned texture scaling issues)
  * show a BMD-ified preview of the model rather than a direct preview?
  * interface for assigning collision behaviors per-material and per-polygon
  * more user-friendly interface

## 3D rendering & co. ##

  * generalize the ObjectRenderer system to everything
  * move GL code out of BMD.cs (will be doable after the aforementioned item is done)
  * put writing code in BMD.cs and KCL.cs to make the model importer code cleaner, and possibly allow for small modifications to existing models

## NitroROM & co. ##

  * make that code less weird
  * make it so that the NARC parsing code isn't redundant with the NitroROM parsing code (aka make a generalized NitroFS class or something)