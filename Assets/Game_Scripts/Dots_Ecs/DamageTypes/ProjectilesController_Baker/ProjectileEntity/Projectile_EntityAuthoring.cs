using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
public struct PlayerProjectileTargetPosData : IComponentData
{
    public float3 playerPositionToSpawn;
    public float3 targetPositionDirection;
}
public struct PlayerProjectileSingletondData : IComponentData
{
    public float speedMultp;
}
public struct PlayerProjectileData : IComponentData
{
    public float speed;
    public float scale;
    public float damage;
    public bool magicalDamage;
    public GlobalData.MagicalDamageType damageType;

    public float criticalChance;

    public PierceData pierceData;
    public int penetratedMonsterCount;
    public CollisionFilter collisionFilter;

    public float TimePassedAfterCreation;

}
public struct ProjectileTag : IComponentData { }
public class Projectile_EntityAuthoring : MonoBehaviour
{
    public BoltProjectileBase ProjectileScriptableObject;
    PhysicsCollider physicsCollider1;

    private void OnDestroy()
    {
        try
        {
            physicsCollider1.Value.Dispose();
        }
        finally
        {
            physicsCollider1.Value.Dispose();
        }
    }

    private void OnDisable()
    {
        try
        {
            physicsCollider1.Value.Dispose();
        }
        finally
        {
            physicsCollider1.Value.Dispose();
        }
    }
    private class Baker : Baker<Projectile_EntityAuthoring>
    {
        public override void Bake(Projectile_EntityAuthoring authoring)
        {

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            MagicalDamageData magicalDamageData = authoring.ProjectileScriptableObject.magicalDamageData;
            BoltProjectileBase boltProjectileScriptableObject = authoring.ProjectileScriptableObject;

            AddComponent(entity, new ProjectileTag());


            AddComponent(entity, new PlayerProjectileData
            {
                scale = boltProjectileScriptableObject.BoltScale,
                speed = boltProjectileScriptableObject.BoltSpeed,

                damage = GlobalModifiersCalculator.DamageCalculator(
                    magicalDamageData.damageType,
                    boltProjectileScriptableObject.BoltDamage,
                    false
                ),

                damageType = magicalDamageData.damageType,

                criticalChance = GlobalModifiersCalculator.DamageCalculator(
                    GlobalData.DamageModifiers.critical,
                    magicalDamageData.criticalChanceData.CriticalHitChance,
                    false
                ),
                penetratedMonsterCount = 0,
                pierceData = new PierceData
                {

                    MagicalPiercingPercantage = GlobalModifiersCalculator.DamageCalculator(
                        GlobalData.ProjectileModifiers.MagicalPiercingPercentage,
                        magicalDamageData.PierceData.MagicalPiercingPercantage,
                        false
                    ),

                    HowManyTimesCanPierceEnemy = GlobalModifiersCalculator.DamageCalculator(
                        GlobalData.ProjectileModifiers.HowManyTimesCanPierceEnemy,
                        magicalDamageData.PierceData.HowManyTimesCanPierceEnemy,
                        false
                    ),

                    CanPierce_AfterittingEnemy = magicalDamageData.PierceData.CanPierce_AfterittingEnemy,
                },

                collisionFilter = GlobalData.FilterProjectile,
            });






            authoring.physicsCollider1 = new PhysicsCollider
            {
                Value = Unity.Physics.SphereCollider.Create(
                 new SphereGeometry
                 {
                     Center = float3.zero,
                     Radius = authoring.ProjectileScriptableObject.BoltScale / 2,
                 },
                 GlobalData.FilterProjectile,
                 new Unity.Physics.Material { CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents })
            };
            var blobAssetReference = BlobAssetReference<Unity.Physics.PhysicsCollider>.Create(authoring.physicsCollider1);

            AddComponent(entity, authoring.physicsCollider1);

            PhysicsMass physicsMass = PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1f);
            AddComponent(entity, physicsMass);

            AddComponent(entity, new PhysicsVelocity
            {
                Linear = float3.zero,
                Angular = float3.zero
            });

            AddComponent(entity, new PhysicsGravityFactor { Value = 0f }); // Disable gravity

            AddSharedComponent(entity, new PhysicsWorldIndex());

        }
    }
}




[UpdateBefore(typeof(EntitiesGraphicsSystem))]
public partial struct ProjectileSystem : ISystem
{
    private NativeHashMap<int, float> projectile_ModifiersHashMap_Current;

    private NativeHashMap<int, float> damage_ModifiersHashMap_Current;

    private NativeHashMap<int, float> damageType_HashMap_Current;

    private bool CanResetHashMaps;
    private float speedMultp;
    public void OnCreate(ref SystemState state)
    {

        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<PlayerProjectileTargetPosData>();
        state.RequireForUpdate<PlayerProjectileData>();
        state.RequireForUpdate<PhysicsVelocity>();
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<PlayerProjectileTargetPosData>();

        GlobalModifiersCalculator.GetModifiersHashMap(typeof(GlobalData.ProjectileModifiers), out projectile_ModifiersHashMap_Current);
        GlobalModifiersCalculator.GetModifiersHashMap(typeof(GlobalData.DamageModifiers), out damage_ModifiersHashMap_Current);
        GlobalModifiersCalculator.GetModifiersHashMap(typeof(GlobalData.MagicalDamageType), out damageType_HashMap_Current);

        GlobalModifiersCalculator.LevelUp_DictionaryChanged_Action += ResetHashMaps;
        speedMultp = 1f;
    }

    private void ResetHashMaps()
    {
        if (projectile_ModifiersHashMap_Current.IsCreated)
            projectile_ModifiersHashMap_Current.Clear();
        if (damage_ModifiersHashMap_Current.IsCreated)
            damage_ModifiersHashMap_Current.Clear();
        if (damageType_HashMap_Current.IsCreated)
            damageType_HashMap_Current.Clear();

        NativeHashMap<int, float> projectile_ModifiersHashMap_Next;
        GlobalModifiersCalculator.GetModifiersHashMap(typeof(GlobalData.ProjectileModifiers), out projectile_ModifiersHashMap_Next);
        NativeHashMap<int, float> damage_ModifiersHashMap_Next;
        GlobalModifiersCalculator.GetModifiersHashMap(typeof(GlobalData.DamageModifiers), out damage_ModifiersHashMap_Next);
        NativeHashMap<int, float> damageType_HashMap_Next;
        GlobalModifiersCalculator.GetModifiersHashMap(typeof(GlobalData.MagicalDamageType), out damageType_HashMap_Next);

        foreach (var item in projectile_ModifiersHashMap_Next)
        {
            projectile_ModifiersHashMap_Current.TryAdd(item.Key, item.Value);
        }
        foreach (var item in damage_ModifiersHashMap_Next)
        {
            damage_ModifiersHashMap_Current.TryAdd(item.Key, item.Value);
        }
        foreach (var item in damageType_HashMap_Next)
        {
            damageType_HashMap_Current.TryAdd(item.Key, item.Value);
        }
        projectile_ModifiersHashMap_Next.Dispose();
        damage_ModifiersHashMap_Next.Dispose();
        damageType_HashMap_Next.Dispose();

        CanResetHashMaps = true;
    }

    public void OnDestroy(ref SystemState state)
    {
        GlobalModifiersCalculator.LevelUp_DictionaryChanged_Action -= ResetHashMaps;

        if (projectile_ModifiersHashMap_Current.IsCreated)
            projectile_ModifiersHashMap_Current.Dispose();
        if (damage_ModifiersHashMap_Current.IsCreated)
            damage_ModifiersHashMap_Current.Dispose();
        if (damageType_HashMap_Current.IsCreated)
            damageType_HashMap_Current.Dispose();
    }


    public void OnUpdate(ref SystemState state)
    {

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var projectileJob = new ProjectileJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ECB = ecb.AsParallelWriter(),
            speedMultp = speedMultp,
        };

        if (CanResetHashMaps)
        {
            foreach (var (Projectile_PrefabData, entity) in SystemAPI.Query<RefRW<Player_Projectile_PrefabData>>().WithEntityAccess())
            {
                Player_Projectile_PrefabData PrefabData = Projectile_PrefabData.ValueRW;
                PrefabData.entityScaleMultply = Projectile_PrefabData.ValueRW.entityScaleBase * projectile_ModifiersHashMap_Current[(int)GlobalData.ProjectileModifiers.BoltColliderScale];
                ecb.SetComponent<Player_Projectile_PrefabData>(entity, PrefabData);
            }

            PlayerProjectileSingletondData playerProjectileSingletondData = SystemAPI.GetSingleton<PlayerProjectileSingletondData>();
            playerProjectileSingletondData.speedMultp = 1f + projectile_ModifiersHashMap_Current[(int)GlobalData.ProjectileModifiers.BoltSpeed];
            speedMultp = playerProjectileSingletondData.speedMultp;
            SystemAPI.SetSingleton(playerProjectileSingletondData);
            CanResetHashMaps = false;
        }




        state.Dependency = projectileJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
        /////////
        /////////
        /////////
        var playerProjectileDataLookup = SystemAPI.GetComponentLookup<PlayerProjectileData>(true);
        var monsterLookup = SystemAPI.GetComponentLookup<MonsterTag>(true);
        var destroyEntityLookup = SystemAPI.GetComponentLookup<DestroyEntity>(true);
        var monsterBaseAttributes = SystemAPI.GetComponentLookup<MonsterBaseAttributes>(true);
        var materialMeshInfoLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>(true);
        var LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);

        playerProjectileDataLookup.Update(ref state);
        monsterLookup.Update(ref state);
        destroyEntityLookup.Update(ref state);
        monsterBaseAttributes.Update(ref state);

        ProjectileData_Indexes_ForJob projectileData_Indexes_ForJob = new ProjectileData_Indexes_ForJob
        {
            ProjectileModifiers = new ProjectileData_Indexes_ForJob.ProjectileModifiersIndixes(0),
            DamageTypes = new ProjectileData_Indexes_ForJob.DamageTypeIndixes(0),
            DamageModifiers = new ProjectileData_Indexes_ForJob.DamageModifiersIndixes(0)
        };

        uint seed = (uint)System.DateTime.Now.Ticks;


        var projectileCollisionJob = new ProjectileCollisionJob
        {
            playerProjectileDataLookup = playerProjectileDataLookup,
            monsterLookup = monsterLookup,
            destroyEntity = destroyEntityLookup,
            monsterBaseAttrubuteLookup = monsterBaseAttributes,

            LocalToWorldLookup = LocalToWorldLookup,
            projectile_ModifiersHashMap_ForJob = projectile_ModifiersHashMap_Current.AsReadOnly(),
            damage_ModifiersHashMap_ForJob = damage_ModifiersHashMap_Current.AsReadOnly(),
            damageType_HashMap_ForJob = damageType_HashMap_Current.AsReadOnly(),

            randomGenerator = new Unity.Mathematics.Random(seed),
            projectileDataIndixesForJob = projectileData_Indexes_ForJob,
            ecb = ecb.AsParallelWriter(),

        };
        state.Dependency = projectileCollisionJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        state.Dependency.Complete();

        var counterSingleton = SystemAPI.GetSingleton<MonsterSpawnCounter>();
        var counterEntity = SystemAPI.GetSingletonEntity<MonsterSpawnCounter>();

        foreach (var (destroyEntity, entity) in SystemAPI.Query<DestroyEntity>().WithEntityAccess())
        {
            if (monsterLookup.HasComponent(entity))
            {
                Entity dataHolderEntity = ecb.CreateEntity();
                LootPositionData destroyedMonsterPosition = new LootPositionData { destroyedMonsterPosition = SystemAPI.GetComponent<LocalToWorld>(entity).Position };

                ecb.AddComponent(entity, new AlphaThresholdOverrideTag());
                ecb.RemoveComponent<MonsterTag>(entity);
                ecb.RemoveComponent<DestroyEntity>(entity);

                ecb.AddComponent(dataHolderEntity, destroyedMonsterPosition);
                counterSingleton.CurrentCount--;

            }
            else
            {
                ecb.DestroyEntity(entity);

            }

        }
        try
        {
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        catch (System.Exception)
        {
            ecb.Dispose();
        }



    }
}


public struct LootPositionData : IComponentData
{
    public float3 destroyedMonsterPosition;
}


[BurstCompile]
public partial struct ProjectileJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter ECB;
    public float speedMultp;
    public void Execute(
        [ChunkIndexInQuery] int chunkIndex,
        ref PhysicsVelocity physicsVelocity,
        ref LocalTransform localTransform,
        ref PlayerProjectileData projectileData,
        in PlayerProjectileTargetPosData playerProjectileTargetPosData,
        Entity entity)
    {
        physicsVelocity.Linear = playerProjectileTargetPosData.targetPositionDirection * speedMultp * projectileData.speed;

        projectileData.TimePassedAfterCreation += DeltaTime;

        localTransform.Position.z = 0f;
        localTransform.Rotation = quaternion.identity;


        if (projectileData.TimePassedAfterCreation > 3f)
        {
            ECB.DestroyEntity(chunkIndex, entity);
        }
    }
}



[BurstCompile]
public struct ProjectileCollisionJob : ITriggerEventsJob
{

    [ReadOnly] public ComponentLookup<PlayerProjectileData> playerProjectileDataLookup;
    [ReadOnly] public ComponentLookup<MonsterTag> monsterLookup;
    [ReadOnly] public ComponentLookup<DestroyEntity> destroyEntity;
    [ReadOnly] public ComponentLookup<MonsterBaseAttributes> monsterBaseAttrubuteLookup;
    [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;


    [ReadOnly] public NativeHashMap<int, float>.ReadOnly projectile_ModifiersHashMap_ForJob;
    [ReadOnly] public NativeHashMap<int, float>.ReadOnly damage_ModifiersHashMap_ForJob;
    [ReadOnly] public NativeHashMap<int, float>.ReadOnly damageType_HashMap_ForJob;
    [ReadOnly] public ProjectileData_Indexes_ForJob projectileDataIndixesForJob;
    [ReadOnly] public Unity.Mathematics.Random randomGenerator;
    public EntityCommandBuffer.ParallelWriter ecb;
    [BurstCompile]
    public void Execute(TriggerEvent triggerEvent)
    {
        bool oneOfTheEntitiyDestroyed = destroyEntity.HasComponent(triggerEvent.EntityA) == true || destroyEntity.HasComponent(triggerEvent.EntityB) == true;

        if (!playerProjectileDataLookup.HasComponent(triggerEvent.EntityA) && !monsterLookup.HasComponent(triggerEvent.EntityA) ||
             !playerProjectileDataLookup.HasComponent(triggerEvent.EntityB) && !monsterLookup.HasComponent(triggerEvent.EntityB))
        {
            return;
        }

        bool isProjectileA = playerProjectileDataLookup.HasComponent(triggerEvent.EntityA);
        Entity projectile;
        Entity monster;
        if (playerProjectileDataLookup.HasComponent(triggerEvent.EntityA))
        {
            projectile = triggerEvent.EntityA;
            monster = triggerEvent.EntityB;
        }
        else
        {
            projectile = triggerEvent.EntityB;
            monster = triggerEvent.EntityA;
        }

        if (monsterLookup.HasComponent(monster) == false) return;

        MonsterBaseAttributes monsterAttributes = monsterBaseAttrubuteLookup[monster];
        PlayerProjectileData playerProjectileData = playerProjectileDataLookup[projectile];
        playerProjectileData.penetratedMonsterCount++;
        ecb.SetComponent<PlayerProjectileData>(projectile.Index, projectile, playerProjectileData);
        if (playerProjectileData.penetratedMonsterCount >= playerProjectileData.pierceData.HowManyTimesCanPierceEnemy + projectile_ModifiersHashMap_ForJob[projectileDataIndixesForJob.ProjectileModifiers.HowManyTimesCanPierceEnemy_Index])
        {
            ecb.RemoveComponent<PhysicsCollider>(projectile.Index, projectile);
            ecb.AddComponent(projectile.Index, projectile, new DestroyEntity());
        }

        float MagicalTypeBaseDamage = CalculateMagicalTypeBaseDamage(playerProjectileData.damageType);
        float typeBasedMagicalResistance = CalculateMagicalResistance(playerProjectileData.damageType, monsterAttributes);
        float criticalDamageMultiplier = CalculateCriticalDamage(playerProjectileData.criticalChance, projectileDataIndixesForJob.DamageModifiers.Critical_Index);
        float totalDamage = CalculateTotalDamage(
            playerProjectileData.damage,
            projectile_ModifiersHashMap_ForJob[projectileDataIndixesForJob.ProjectileModifiers.BoltDamage_Index],
            MagicalTypeBaseDamage,
            monsterAttributes.BaseMagicResistance,
            typeBasedMagicalResistance,
            playerProjectileData.pierceData.MagicalPiercingPercantage,
            damage_ModifiersHashMap_ForJob[projectileDataIndixesForJob.DamageModifiers.AllDamageTypes_Index],
            criticalDamageMultiplier);

        float monsterHp = monsterAttributes.HP;
        monsterHp -= totalDamage;


        Entity entity = ecb.CreateEntity(projectile.Index);
        ecb.AddComponent<TotalDamageToMonsters>(projectile.Index, entity);
        ecb.SetComponent(projectile.Index, entity, new TotalDamageToMonsters { totalDamage = totalDamage, location = LocalToWorldLookup.GetRefRO(monster).ValueRO.Position });



        if (monsterHp <= 0f)
        {
            ecb.RemoveComponent<PhysicsCollider>(projectile.Index, monster);
            ecb.AddComponent(projectile.Index, monster, new DestroyEntity());
        }

        monsterAttributes.HP = monsterHp;
        ecb.SetComponent(projectile.Index, monster, monsterAttributes);
    }

    [BurstCompile]
    private float CalculateMagicalResistance(GlobalData.MagicalDamageType damageType, MonsterBaseAttributes monsterAttributes)
    {
        switch (damageType)
        {
            case GlobalData.MagicalDamageType.Bleed: return monsterAttributes.BleedResistance;
            case GlobalData.MagicalDamageType.Poison: return monsterAttributes.PoisonResistance;
            case GlobalData.MagicalDamageType.Burn: return monsterAttributes.BurnResistance;
            case GlobalData.MagicalDamageType.Freeze: return monsterAttributes.FrozenResistance;
            case GlobalData.MagicalDamageType.Shock: return monsterAttributes.ShockResistance;
            default: return 0;
        }
    }
    [BurstCompile]
    private float CalculateMagicalTypeBaseDamage(GlobalData.MagicalDamageType damageType)
    {
        return damageType_HashMap_ForJob[(int)damageType];
    }
    [BurstCompile]
    private float CalculateCriticalDamage(float criticalChance, float criticalModifier)
    {
        return randomGenerator.NextFloat() < criticalChance / 100 ? criticalModifier : 1f;
    }
    [BurstCompile]
    private float CalculateTotalDamage(
    float baseDamage,
    float boltDamageModifier,
    float typeBaseDamage,
    float magicResistance,
    float typeBasedResistance,
    float magicalPiercingPercentage,
    float allDamageModifier,
    float criticalDamage)
    {
        float damageAfterModifiers = (baseDamage * boltDamageModifier) * typeBaseDamage;

        float resistance = ((magicResistance + typeBasedResistance) - magicalPiercingPercentage);
        if (resistance == 0)
        {
            return (damageAfterModifiers) * allDamageModifier * criticalDamage;
        }
        return (damageAfterModifiers * 100f - resistance) * allDamageModifier * criticalDamage;
    }


}
public struct TotalDamageToMonsters : IComponentData
{
    public float totalDamage;
    public float3 location;
}
public struct DestroyEntity : IComponentData { }

public partial class MonsterAlphaSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (SystemAPI.TryGetSingleton<MonsterSpawnCounter>(out MonsterSpawnCounter counterSingleton))
        {
            var counterEntity = SystemAPI.GetSingletonEntity<MonsterSpawnCounter>();
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            float deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (thresholdOverride, entity) in SystemAPI.Query<RefRW<AlphaThresholdOverride>>().WithEntityAccess().WithAll<AlphaThresholdOverrideTag>())
            {
                var renderer = SystemAPI.ManagedAPI.GetComponent<SpriteRenderer>(entity);
                thresholdOverride.ValueRW.materialAlphaThreshold += 1f * deltaTime;
                renderer.material.SetFloat("_CUTCUTCUT", thresholdOverride.ValueRW.materialAlphaThreshold);
                if (thresholdOverride.ValueRW.materialAlphaThreshold >= 1f)
                    ecb.DestroyEntity(entity);
                counterSingleton.CurrentCount--;

            }

            ecb.SetComponent(counterEntity, counterSingleton);
            ecb.Playback(World.EntityManager);
            ecb.Dispose();

        }

    }
}

public struct ProjectileData_Indexes_ForJob
{
    public struct ProjectileModifiersIndixes
    {
        public int BoltSpeed_Index;
        public int BoltDamage_Index;
        public int BoltColliderScale_Index;
        public int CoolDown_Index;
        public int PhysicalPiercingPercentage_Index;
        public int MagicalPiercingPercentage_Index;
        public int HowManyTimesCanPierceEnemy_Index;

        public ProjectileModifiersIndixes(int startIndex)
        {
            BoltSpeed_Index = startIndex++;
            BoltDamage_Index = startIndex++;
            BoltColliderScale_Index = startIndex++;
            CoolDown_Index = startIndex++;
            PhysicalPiercingPercentage_Index = startIndex++;
            MagicalPiercingPercentage_Index = startIndex++;
            HowManyTimesCanPierceEnemy_Index = startIndex++;
        }
    }

    public struct DamageTypeIndixes
    {
        public int Standard_Index;
        public int Bleed_Index;
        public int Poison_Index;
        public int Burn_Index;
        public int Froze_Index;
        public int Shock_Index;

        public DamageTypeIndixes(int startIndex)
        {
            Standard_Index = startIndex++;
            Bleed_Index = startIndex++;
            Poison_Index = startIndex++;
            Burn_Index = startIndex++;
            Froze_Index = startIndex++;
            Shock_Index = startIndex++;
        }
    }

    public struct DamageModifiersIndixes
    {
        public int AllDamageTypes_Index;
        public int Critical_Index;

        public DamageModifiersIndixes(int startIndex)
        {
            AllDamageTypes_Index = startIndex++;
            Critical_Index = startIndex++;
        }
    }

    public ProjectileModifiersIndixes ProjectileModifiers;
    public DamageTypeIndixes DamageTypes;
    public DamageModifiersIndixes DamageModifiers;

    public ProjectileData_Indexes_ForJob(int startIndex = 0)
    {
        ProjectileModifiers = new ProjectileModifiersIndixes(startIndex);
        DamageTypes = new DamageTypeIndixes(startIndex);
        DamageModifiers = new DamageModifiersIndixes(startIndex);
    }

}