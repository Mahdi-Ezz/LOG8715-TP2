using Unity.Entities;
using Unity.Mathematics;

public struct PlantTag : IComponentData { }
public struct PreyTag : IComponentData { }
public struct PredatorTag : IComponentData { }

public struct VelocityComponent : IComponentData
{
    public float3 Value;
}

// Composant lifetime
public struct LifetimeComponent : IComponentData
{
    public float Current;
    public float Starting;
    public float DecreasingFactor;
    public bool Reproduced;
    public bool AlwaysReproduce;

    public float GetProgression() => Current / Starting;
}

// Composant pour signaler qu'une entité doit être respawnée
//public struct NeedsRespawnTag : IComponentData { }

// Composant pour la taille visuelle (plant scale)
public struct ScaleComponent : IComponentData
{
    public float Value;
}
