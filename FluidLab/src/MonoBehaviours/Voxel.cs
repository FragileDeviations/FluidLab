using System;

using SystemVector3 = System.Numerics.Vector3;

namespace FluidLab;

[Serializable]
public class Voxel
{
    public SystemVector3 position;

    public float submersion;
}