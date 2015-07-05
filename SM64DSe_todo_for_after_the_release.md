## Before anything ##

All of this will be done if and only if the oncoming release gives SM64DSe some interest. Otherwise we'll just call this a failure and move on.

## General ##

  * Move to Github <br>
No offence meant to you GoogleCode guys but Github is cooler.</li></ul>

<ul><li>Rewrite the editor in Java<br>
C# and Linux just don't go together. Especially when you try to use OpenGL.</li></ul>

Java works better in that regard. Fiachra will have to give his opinion on this, though. I'm not going to do it if he has a problem with Java.

## Renderer ##

  * Refactor the BMD stuff: separate parsing from rendering, like how it's done in Whitehole.

  * Oh and integrate BMD saving in the BMD class too, so that we can remove that huge chunk of crap from the model importer.

  * Find a better way to assign renderers to objects than that huge switch statement. Most of the renderers being assigned are NormalBMDRenderers with a scale of 0.008.

## Level editor ##

  * Make 3D interaction more intelligent.

Retrofit the improvements that were made to Whitehole's interface. For example, how mousewheel scrolling zooms towards where the user is pointing rather than towards the screen center.

## File access and all ##

  * Use Whitehole's system?

The current system works, but it's not really pretty. It's also not outright compatible with Java's File IO APIs.

Oh and Whitehole's system allows for more flexibility.

  * Use common classes for things like NitroFS, rather than copypasting the same code in different classes (think NitroROM/NARC)