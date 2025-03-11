using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct ProjectileEntitySpawner : ISystem
{
    NativeHashMap<Entity, float> castSpawnerEntities_CooldownPair;


    public void OnCreate(ref SystemState state)
    {
        castSpawnerEntities_CooldownPair = new NativeHashMap<Entity, float>(16, Allocator.Persistent);
    }


    public void OnDestroy(ref SystemState state)
    {
        castSpawnerEntities_CooldownPair.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {

        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (playerProjectileData, entity) in
                 SystemAPI.Query<Player_Projectile_PrefabData>().WithEntityAccess().WithAll<ProjectileSpawner>())
        {
            if (castSpawnerEntities_CooldownPair.ContainsKey(entity))
            {
                if (playerProjectileData.coolDown <= castSpawnerEntities_CooldownPair[entity])
                {
                    castSpawnerEntities_CooldownPair[entity] = 0f;

                    SpawnProjectiles(playerProjectileData, ecb, ref state);
                }
                else
                {
                    castSpawnerEntities_CooldownPair[entity] += deltaTime;
                }
            }
            else
            {

                castSpawnerEntities_CooldownPair.Add(entity, 0f);
            }
        }
    }

    public void SpawnProjectiles(Player_Projectile_PrefabData _PrefabData, EntityCommandBuffer ecb, ref SystemState state)
    {

        Entity projectileEntity = ecb.Instantiate(_PrefabData.ProjectilePrefab);
        float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>()).Position;

        NativeReference<float> ClosestDistance = new NativeReference<float>(Allocator.TempJob);
        NativeReference<float3> closestMonsterPos = new NativeReference<float3>(Allocator.TempJob);
        ClosestDistance.Value = float.MaxValue;
        closestMonsterPos.Value = new float3(float.MaxValue);

        var job = new FindClosestMonster
        {
            PlayerPosition = playerPosition,
            ClosestDistance = ClosestDistance,
            ClosestMonsterPositionDirection = closestMonsterPos,
        };

        var jobhandle = job.Schedule(state.Dependency);
        jobhandle.Complete();


        ecb.SetComponent(projectileEntity, new LocalTransform
        {
            Position = playerPosition,
            Rotation = quaternion.identity,
            Scale = _PrefabData.entityScaleMultply,
        });

        /* ecb.SetComponent(projectileEntity, new LocalToWorld
         {
             Value = float4x4.TRS(playerPosition, quaternion.identity, new float3(5, 5, 5))
         });*/
        float3 targetPositionDirection;
        if (!closestMonsterPos.Value.Equals(float.MaxValue))
        {
            targetPositionDirection = closestMonsterPos.Value;
        }
        else
        {
            targetPositionDirection = new float3(1f,0f,0f);
        }

        ecb.AddComponent(projectileEntity, new PlayerProjectileTargetPosData { playerPositionToSpawn = playerPosition, targetPositionDirection = targetPositionDirection });
        ClosestDistance.Dispose();
        closestMonsterPos.Dispose();

    }
}

[BurstCompile]
public partial struct FindClosestMonster : IJobEntity
{
    public float3 PlayerPosition;
    public NativeReference<float> ClosestDistance;
    public NativeReference<float3> ClosestMonsterPositionDirection;
    public void Execute(in LocalTransform transform, in MonsterTag monsterTag, Entity entity)
    {
        float distance = math.distance(PlayerPosition, transform.Position);
        if (distance < ClosestDistance.Value)
        {
            ClosestDistance.Value = distance;
            ClosestMonsterPositionDirection.Value = math.normalize(transform.Position - PlayerPosition);
        }
    }

}