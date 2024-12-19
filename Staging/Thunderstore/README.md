Adds voxel based fluid dynamics to BONELAB, such as water buoyancy and drag.

# FLUIDLAB
Ever wished that the waters of Tuscany had accurate water physics instead of just killing you? This mod provides those, and more!

## CONTENT
* Liquid Buoyancy
* Liquid Drag
* Liquid Volumes in the following levels:
    * Tuscany (Water)
    * Dungeon Warrior (Lava)
    * Magma Gate (Lava)
    * Ascent (Acid)

## FLUID SDK
Modders can implement their own fluid physics in their maps using the Fluid SDK.

This is available [on the GitHub.](https://github.com/Lakatrazz/FluidLab/releases/latest/)

## CAVEATS
* BONELAB has semi-realistic masses for small objects. However, many of the large objects are very light compared to what they should be, meaning they will float in the water due to it using density instead of a simple mass check. If you want these larger objects to sink as they should, it is recommended to use a mod that increases their weight to realistic amounts.

## DEPENDENCIES
- BoneLib: https://bonelab.thunderstore.io/package/gnonme/BoneLib/

## MEDIA