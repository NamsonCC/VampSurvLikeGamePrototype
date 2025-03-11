using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private class Baker : Baker<ProjectileController>
    {
        public override void Bake(ProjectileController authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new ProjectileController_Tag { });
            AddComponent(entity, new PlayerProjectileSingletondData { speedMultp = 1f });
        }
    }
}

public struct ProjectileController_Tag : IComponentData { }
[BurstCompile]
public partial struct ProjectileControllerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {

    }
}