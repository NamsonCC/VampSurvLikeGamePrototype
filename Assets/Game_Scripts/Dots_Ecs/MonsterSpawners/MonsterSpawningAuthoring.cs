using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MonsterSpawningAuthoring : MonoBehaviour
{


    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private GameObject[] prefabDataEntities;

    public float spawnRadius;
    public int MaxEntityCount;
    public int CurrentCountOfEntitites;
    public float scale;
    private class Baker : Baker<MonsterSpawningAuthoring>
    {
        public override void Bake(MonsterSpawningAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MonsterSpawnCounter { CurrentCount = authoring.CurrentCountOfEntitites, MaxCount = authoring.MaxEntityCount, scale = authoring.scale, spawnRadius = authoring.spawnRadius, minIntervalTime = 0.1f, maxIntervalTime = 3f, timeInterval = 0f, Timer = 0f });
            for (int i = 0; i < authoring.prefabDataEntities.Length; i++)
            {
                authoring.prefabs[i].GetComponent<MonsterMovement_Authering>().monsterPrefabIndex = i;
                authoring.prefabDataEntities[i].GetComponent<MonsterSpawnable_Authorizer>().index = i;
            }
        }
    }
}



[BurstCompile]
public partial struct MonsterSpawnSystem : ISystem
{
    NativeList<MonsterPrefab> monsterPrefabEntities;
    private bool monsterPrefabsFinded;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MonsterPrefab>();
        state.RequireForUpdate<PlayerMovementComponent>();
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<MonsterSpawnCounter>();
        monsterPrefabsFinded = false;
        monsterPrefabEntities = new NativeList<MonsterPrefab>(Allocator.Persistent);

    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        monsterPrefabEntities.Dispose();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {


        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var counterSingleton = SystemAPI.GetSingleton<MonsterSpawnCounter>();
        var counterEntity = SystemAPI.GetSingletonEntity<MonsterSpawnCounter>();

      // Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)Time.frameCount);
        counterSingleton.Timer += SystemAPI.Time.DeltaTime;

        if (counterSingleton.timeInterval <= 0f)
        {
            counterSingleton.timeInterval = random.NextFloat(counterSingleton.minIntervalTime, counterSingleton.maxIntervalTime);
        }
        else
        {
            counterSingleton.timeInterval -= SystemAPI.Time.DeltaTime;
            ecb.SetComponent(counterEntity, counterSingleton);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            return;
        }
        if (counterSingleton.CurrentCount >= counterSingleton.MaxCount)
        {
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            return;
        }




        // Player'ýn pozisyonu
        float3 playerPosition = float3.zero;
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerMovementComponent>();
        LocalTransform localTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
        playerPosition = localTransform.Position;

        // Monster spawnlama
        float angleStep = math.PI * 2f / 100f;
   

        if (monsterPrefabsFinded == false )
        {

            foreach (var monsterPrefab in SystemAPI.Query<MonsterPrefab>())
            {
                monsterPrefabEntities.Add(monsterPrefab);
            }
            monsterPrefabsFinded = true;
        }
        //Debug.Log("Monster prefabs finded passed");

        int rand = random.NextInt(0, monsterPrefabEntities.Length);

        if (counterSingleton.CurrentCount <= counterSingleton.MaxCount)
        {
            int maxIterationCount = (int)CalculateY((counterSingleton.Timer / 60f));
            for (int i = 0; i < maxIterationCount; i++)
            {
                float angle = ((i * 100) / maxIterationCount) * angleStep;
                float x = playerPosition.x + math.cos(angle) * 10f; 
                float y = playerPosition.y + math.sin(angle) * 10f;
                float randomFloat1 = random.NextFloat(1.3f, 1.5f);

                float randomFloat2 = random.NextFloat(1.3f, 1.5f);

                float3 spawnPosition = new float3(x * randomFloat1, y * randomFloat2, 0f);


                var newMonster = ecb.Instantiate(monsterPrefabEntities[rand].PrefabEntity);
                ecb.SetComponent(newMonster, new LocalTransform
                {
                    Position = spawnPosition,
                    Rotation = quaternion.identity,
                    Scale = counterSingleton.scale,
                });

                counterSingleton.CurrentCount++;
            }
        }
        if (counterSingleton.Timer % 5 == 0 && counterSingleton.Timer > 4f)
        {
            int maxIterationCount = 300;
            for (int i = 0; i < maxIterationCount; i++)
            {
                float angle = ((i * 100) / maxIterationCount) * angleStep;
                float x = playerPosition.x + math.cos(angle) * 10f; 
                float y = playerPosition.y + math.sin(angle) * 10f;
                float randomFloat1 = random.NextFloat(1.5f, 4f);
                float randomFloat2 = random.NextFloat(1.5f, 4f);


                float3 spawnPosition = new float3(x * randomFloat1, y * randomFloat2, 0f);

                var newMonster = state.EntityManager.Instantiate(monsterPrefabEntities[rand].PrefabEntity);

                ecb.SetComponent(newMonster, new LocalTransform
                {
                    Position = spawnPosition,
                    Rotation = quaternion.identity,
                    Scale = counterSingleton.scale,
                });
            }
        }

        ecb.SetComponent(counterEntity, counterSingleton);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    [BurstCompile]
    public float CalculateY(float x)
    {
        float a = 20f;
        float b = 0.16f;
        float c = 5f;

        float y = a * Mathf.Exp(b * x) + c;
        return y;
    }
}

public struct MonsterSpawnCounter : IComponentData
{
    public int MaxCount;
    public int CurrentCount;
    public float scale;
    public float spawnRadius;
    public float minIntervalTime;
    public float maxIntervalTime;
    public float timeInterval;
    public float Timer;

}
