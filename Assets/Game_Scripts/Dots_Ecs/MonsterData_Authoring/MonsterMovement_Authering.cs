using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


[MaterialProperty("_CUTCUTCUT")]
public struct AlphaThresholdOverride : IComponentData
{
    public float materialAlphaThreshold;
}
public struct AlphaThresholdOverrideTag : IComponentData { }

public struct MonsterIdentity : IComponentData
{
    public int monsterPrefabIndex;
    public int MonsterTextureNumber;
    public GlobalData.RarityDegree Rarity;
}
public struct MonstersMovingComponent : IComponentData
{
    public float Scale;
    public float Speed;
    public float SeparationRadius;
    public bool IsMovementBlocked;
}


public struct MonsterBaseAttributes : IComponentData
{

    public float HP;
    public float HP_Regeneration;
    public float BaseMagicResistance;
    public float BaseDamage;
    public float DamageCooldown_Limit;
    public float DamageCooldown;


    public float BleedResistance;
    public float PoisonResistance;
    public float BurnResistance;
    public float FrozenResistance;
    public float ShockResistance;

    public bool UnBleedable;
    public bool UnPoisonable;
    public bool UnBurnable;
    public bool UnFrozenable;
    public bool UnShockable;

}

public struct ScaleData : IComponentData
{
    public float MinScaleX;
    public float MaxScaleX;
    public float Speed;
    public float Time;
}

public struct RotationData : IComponentData
{
    public float Angle;
    public float Speed;
    public float Time;
}

public struct MonsterTag : IComponentData { }
public class MonsterMovement_Authering : MonoBehaviour
{
    public MonsterData monsterData;
    public int monsterPrefabIndex;
    public float speed;
    public float Radius;
    private static BlobAssetReference<Unity.Physics.Collider> sharedCollider;

    public float MinScaleX = 1.95f;
    public float MaxScaleX = 2f;
    public float ScaleSpeed = 5.0f;
    public float RotationAngle = 8.0f; // 5 derece
    public float RotationSpeed = 5.0f;
    private class Baker : Baker<MonsterMovement_Authering>
    {
        public override void Bake(MonsterMovement_Authering authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<MonsterTag>(entity);
            #region MonsterData

            AddComponent(entity, new MonsterIdentity
            {
                monsterPrefabIndex = authoring.monsterPrefabIndex,
                MonsterTextureNumber = authoring.monsterData.monsterTextureNumber,
                Rarity = authoring.monsterData.Rarity,
            });
            AddComponent(entity, new MonstersMovingComponent
            {
                Scale = authoring.monsterData.Scale,
                Speed = authoring.monsterData.Speed,
                SeparationRadius = authoring.monsterData.SeparationRadius,
                IsMovementBlocked = authoring.monsterData.IsMovementBlocked,
            }); ;
            AddComponent(entity, new MonsterBaseAttributes
            {
                HP = authoring.monsterData.HP,
                BaseMagicResistance = authoring.monsterData.BaseMagicResistance,
                BaseDamage = authoring.monsterData.BaseDamage,
                DamageCooldown_Limit = authoring.monsterData.DamageCooldown_Limit,
                DamageCooldown = authoring.monsterData.DamageCooldown,

                BleedResistance = authoring.monsterData.BleedResistance,
                PoisonResistance = authoring.monsterData.PoisonResistance,
                BurnResistance = authoring.monsterData.BurnResistance,
                FrozenResistance = authoring.monsterData.FrozenResistance,
                ShockResistance = authoring.monsterData.ShockResistance,

                UnBleedable = authoring.monsterData.UnBleedable,
                UnPoisonable = authoring.monsterData.UnPoisonable,
                UnBurnable = authoring.monsterData.UnBurnable,
                UnFrozenable = authoring.monsterData.UnFrozenable,
                UnShockable = authoring.monsterData.UnShockable,


            });

            AddComponent(entity, new ScaleData
            {
                MinScaleX = authoring.MinScaleX,
                MaxScaleX = authoring.MaxScaleX,
                Speed = authoring.ScaleSpeed,
                Time = 0
            });
            AddComponent(entity, new RotationData
            {
                Angle = authoring.RotationAngle,
                Speed = authoring.RotationSpeed,
                Time = 0
            });


            #endregion
            #region EntityMaterialDeclaration
            AddComponent(entity, new AlphaThresholdOverride { materialAlphaThreshold = 0.01f });

            //AddComponentObject<MaterialOverride>(entity, new MaterialOverride());


            #endregion


            #region Physics

            if (!sharedCollider.IsCreated)
            {
                sharedCollider = Unity.Physics.SphereCollider.Create(
                    new SphereGeometry
                    {
                        Center = float3.zero,
                        Radius = 0.1f
                    },
                    GlobalData.FilterMonster
                );
            }

            AddComponent(entity, new PhysicsCollider
            {
                Value = sharedCollider
            });
            PhysicsMass physicsMass = PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1f);
            AddComponent(entity, physicsMass);

            AddComponent(entity, new PhysicsVelocity
            {
                Linear = float3.zero,
                Angular = float3.zero
            });
            AddSharedComponent(entity, new PhysicsWorldIndex());

            #endregion
        }
    }

    private void OnDisable()
    {
        Debug.LogError("This must change to game finished not on disable");
        sharedCollider.Dispose();
    }
}




[BurstCompile]
public partial struct MonsterMoveSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MonsterBaseAttributes>();
        state.RequireForUpdate<PlayerMovementComponent>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float3 playerPosition = float3.zero;
        foreach (var playerTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerMovementComponent>())
        {
            playerPosition = playerTransform.ValueRO.Position;
            break;
        }

        if (SystemAPI.TryGetSingleton<PlayerData>(out var playerDataComponent))
        {

            NativeQueue<float> playerHpDamageQueue = new NativeQueue<float>(Allocator.TempJob);

            Monster_PlayerTrackerJob monster_PlayerTrackerJob = new Monster_PlayerTrackerJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                playerHpDamageQueue = playerHpDamageQueue,
                PlayerPosition = playerPosition,

            };


            state.Dependency = monster_PlayerTrackerJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            Monster_PlayerTrackerJobFor_AlphaThresholdOverride AlphaThresholdOverrideJob = new Monster_PlayerTrackerJobFor_AlphaThresholdOverride();
            state.Dependency = AlphaThresholdOverrideJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            float totalDamage = 0f;



            // Iterate through the queue and accumulate the damage values
            while (playerHpDamageQueue.Count > 0)
            {
                totalDamage += playerHpDamageQueue.Dequeue();

            }

            if (playerDataComponent.HP < 1)
            {
                GameFinishedPlayerDead(ref state);
            }
            else
            {
                playerDataComponent.HP -= totalDamage;

                SystemAPI.SetSingleton(playerDataComponent);
            }


            playerHpDamageQueue.Dispose();

            //World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FixedStepSimulationSystemGroup>().Update(state.WorldUnmanaged);
            WorldPhysicsConst(ref state);

        }
        else
        {
          //  Debug.Log("playerDataComponent did not find ");
        }
    }
    [BurstDiscard]
    private void WorldPhysicsConst(ref SystemState state)
    {
        var physicsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FixedStepSimulationSystemGroup>();
        physicsSystem.Update(state.WorldUnmanaged);
    }
    [BurstDiscard]
    private void GameFinishedPlayerDead(ref SystemState state)
    {
        Time.timeScale = 0;
        IngameMenuControler.Instance.EnableGameOverMenu();
        Debug.Log("Player dead");
    }
}

[BurstCompile]
public partial struct Monster_PlayerTrackerJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public NativeQueue<float> playerHpDamageQueue;
    [ReadOnly] public float3 PlayerPosition;
    [ReadOnly] public float deltaTime;

    //In readonly
    //ref read write
    public void Execute(ref LocalTransform localTransform, in MonstersMovingComponent movingComponent,
        ref MonsterBaseAttributes monsterBaseAttributes, RefRW<PhysicsVelocity> physicsVelocity, in MonsterTag monster)
    {
        float3 difference = PlayerPosition - localTransform.Position;
        localTransform.Rotation = Quaternion.identity;
        if (math.length(difference) >= 1f)
        {
            if (movingComponent.IsMovementBlocked == false)
            {
                physicsVelocity.ValueRW.Linear = math.normalize(difference) * movingComponent.Speed; 
            }
        }
        else
        {
            if (monsterBaseAttributes.DamageCooldown_Limit < monsterBaseAttributes.DamageCooldown)
            {
                playerHpDamageQueue.Enqueue(monsterBaseAttributes.BaseDamage);
                monsterBaseAttributes.DamageCooldown = 0f;
            }
            else
            {
                monsterBaseAttributes.DamageCooldown += deltaTime;
            }
        }
    }
}
[BurstCompile]
public partial struct Monster_PlayerTrackerJobFor_AlphaThresholdOverride : IJobEntity
{
    //In readonly
    //ref read write
    public void Execute(in AlphaThresholdOverride alphaThresholdOverride, ref LocalTransform localTransform, ref PhysicsVelocity physicsVelocity, in AlphaThresholdOverrideTag tag)
    {
        physicsVelocity.Linear = float3.zero;
        localTransform.Rotation = Quaternion.identity;
    }
}