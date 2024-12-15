using UnityEngine;

namespace FluidLab;

public struct LiquidDefinition
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 size;
    public float density;

    public LiquidDefinition(Vector3 position, Quaternion rotation, Vector3 size, float density = 1000f)
    {
        this.position = position;
        this.rotation = rotation;
        this.size = size;
        this.density = density;
    }
}

public static class LiquidCreator
{
    public static LiquidVolume CreateLiquid(Vector3 position, Quaternion rotation, Vector3 size, float density = 1000f) 
    {
        var liquid = new GameObject("Liquid Volume");
        liquid.transform.position = position;
        liquid.transform.rotation = rotation;

        var trigger = liquid.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = size;

        var volume = liquid.AddComponent<LiquidVolume>();
        volume.size.Set(size);
        volume.density.Set(density);

        return volume;
    }

    public static FlowVolume CreateFlow(Vector3 position, Quaternion rotation, Vector3 size, Vector3 flow)
    {
        var flowObject = new GameObject("Flow Volume");
        flowObject.transform.position = position;
        flowObject.transform.rotation = rotation;

        var trigger = flowObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = size;

        var volume = flowObject.AddComponent<FlowVolume>();
        volume.size.Set(size);
        volume.flow.Set(flow);

        return volume;
    }
}