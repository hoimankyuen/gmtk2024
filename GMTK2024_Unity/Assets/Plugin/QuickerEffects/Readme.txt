==== Quicker Effects ===

This is an improved version of both quick outline and quick overlay that supports
models with multiple materials.

For usage, please refer to the readme file in their respective folder.

Versions:
1.0:   First release
1.1:   Add QuickBlinker into bundle. Change name to Quicker Effects
1.2:   Move all scripts into namespace QuickEffects
1.3:   Added shader level enabled / disable for all effects
1.4:   Make value set do not update when recieving the same value, make parameters more consistant with others.
1.5:   Update QuickBlinker in attempt to make it more user friendly.
1.6.0: Update QuickBlinker to include direction options.
1.6.1: Remove metas to solve importing problem in unity 2018
1.7.0: It is now possible to break the parent-child chaining of the effects by adding the same effect component on the root object
       that the effect is not desired.
1.7.1: Fix QuickOutline not correctly display when two object, one with show = true and one with false are shown in close proximity.
1.8.0: It is now possible to break the parent-child chaining and prevent effect materials being added by adding the EffectStopper 
	   component on the root object that the effects are not desired.
1.8.1: Fix issue of blinker render incorrectly when using fog.
1.8.2: Fix the one frame blink when an item with the effect is spawned.
1.8.3: Add option to prevent smoothing normal for outline.
1.9.0: Allow adding and removing renderer after effects are shown.
	   Add Refresh function for recalculate effects after adding renderers.
1.9.1: Fix exceptions caused by adding effect to particle systems.