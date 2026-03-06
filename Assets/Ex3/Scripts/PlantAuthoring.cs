using System;
using Unity.Entities;
using UnityEngine;

public class PlantAuthoring : MonoBehaviour
{
    class Baker : Baker<PlantAuthoring>
    {
        public override void Bake(PlantAuthoring authoring)
        {
            // The entity will be moved
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlantTag>(entity);
            var starting = UnityEngine.Random.Range(5, 15);
            AddComponent(entity, new LifetimeComponent
            {
                Starting = starting,
                Current = starting,
                DecreasingFactor = 1f,
                AlwaysReproduce = true,
                Reproduced = false
            });

            AddComponent(entity, new ScaleComponent
            {
                Value = 1
            });
        }
    }
}
