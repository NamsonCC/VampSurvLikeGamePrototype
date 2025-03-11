using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ProjectilePrefabData_Authorizer : MonoBehaviour
{
    public GameObject ProjectilePrefab;

    public class Baker : Baker<ProjectilePrefabData_Authorizer>
    {

        public override void Bake(ProjectilePrefabData_Authorizer authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            Entity prefabEntity = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.None);
            AddComponent(entity,new ProjectileSpawner());
            AddComponent(entity, new Player_Projectile_PrefabData
            {
                ProjectilePrefab = prefabEntity,
                coolDown = authoring.ProjectilePrefab.GetComponent<Projectile_EntityAuthoring>().ProjectileScriptableObject.CoolDown,
                entityScaleMultply = authoring.ProjectilePrefab.GetComponent<Projectile_EntityAuthoring>().ProjectileScriptableObject.BoltScale,
                entityScaleBase = authoring.ProjectilePrefab.GetComponent<Projectile_EntityAuthoring>().ProjectileScriptableObject.BoltScale,
                speedMultply = authoring.ProjectilePrefab.GetComponent<Projectile_EntityAuthoring>().ProjectileScriptableObject.BoltSpeed,
                speedBase = authoring.ProjectilePrefab.GetComponent<Projectile_EntityAuthoring>().ProjectileScriptableObject.BoltSpeed,
            });
        }
    }
}

public struct ProjectileSpawner: IComponentData { }
public struct Player_Projectile_PrefabData : IComponentData
{
    public Entity ProjectilePrefab;
    public float coolDown;
    public float entityScaleMultply;
    public float entityScaleBase;
    public float speedMultply;
    public float speedBase;

}

