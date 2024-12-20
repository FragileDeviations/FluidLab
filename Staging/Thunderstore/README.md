Adds voxel based fluid dynamics to BONELAB, such as water buoyancy and drag.

# IMPORTANT
For splash effects, ambience, and more to work in the base game levels, you NEED to download the [Fluid Effects resource off of mod.io!](https://mod.io/g/bonelab/m/fluid-effects-resource)

This is a regular SDK mod, meaning you can download it to your SDK mods folder or by subscribing to it and clicking Download All in-game, but it cannot be downloaded to your MelonLoader/LemonLoader mods folder!

# FLUIDLAB
Ever wished that the waters of Tuscany had accurate water physics instead of just killing you? This mod provides those, and more!

## CONTENT
* Liquid Buoyancy
* Liquid Drag (Includes swimming!)
* Liquid Volumes in the following levels:
    * Tuscany (Water)
    * Dungeon Warrior (Lava)
    * Magma Gate (Lava)
    * Ascent (Acid)

## CONTROLS
While in a liquid, the player can control their buoyancy by tilting up or down on the virtual crouch thumbstick. This will allow you to move up or down in the water without having to swim.

## FLUID SDK
Modders can implement their own fluid physics in their maps using the Fluid SDK.

This is available [on the GitHub.](https://github.com/Lakatrazz/FluidLab/releases/latest/)

## CAVEATS
* BONELAB has semi-realistic masses for small objects. However, many of the large objects are very light compared to what they should be, meaning they will float in the water due to it using density instead of a simple mass check. If you want these larger objects to sink as they should, it is recommended to use a mod that increases their weight to realistic amounts.

## DEPENDENCIES
- BoneLib: https://bonelab.thunderstore.io/package/gnonme/BoneLib/
- Fluid Effects (RESOURCE): https://mod.io/g/bonelab/m/fluid-effects-resource/

## MEDIA
![Buoyancy](https://i.imgur.com/pNrjXWl.gif)

All objects have a certain buoyancy based on their mass and volume.

<br>

![Drag](https://i.imgur.com/HmMUmDl.gif)

Simulated drag allows for you to create motors to propel your creations forward.

<br>

![Swimming](https://i.imgur.com/2tfa266.gif)

While in the water, you can swim quickly by moving your hands, or holding up/down on the thumbstick to ascend/descend.

[![Video](https://i.imgur.com/ydJfVWu.png)](https://www.youtube.com/watch?v=blaqg_cJdl4)