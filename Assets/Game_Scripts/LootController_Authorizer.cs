using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LootController_Authorizer : MonoBehaviour
{
    public GameObject LootItemPrefab;
    public GameObject[] XpEntities;
    private class Baker : Baker<LootController_Authorizer>
    {
        public override void Bake(LootController_Authorizer authoring)
        {

            Entity entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<LootController_XpLootBufferData>(entity);
            int index = 0;
            foreach (var XpEntity in authoring.XpEntities)
            {

                EntitiesAndIndexes lootController_XpLootBufferData = new EntitiesAndIndexes
                {
                    LootType = (int)LootController.LootTypes.XP,
                    LootIndexByType = index,
                    LootEntity = GetEntity(authoring.XpEntities[index], TransformUsageFlags.Dynamic)

                };
                buffer.Add(new LootController_XpLootBufferData { entitiesAndIndexes = lootController_XpLootBufferData });
                index++;
            }
            Debug.LogError("buffer.Add(new LootController_XpLootBufferData { entitiesAndIndexes = lootController_XpLootBufferData });");
            Entity prefabEntity = GetEntity(authoring.LootItemPrefab, TransformUsageFlags.Dynamic);
            Entity lootItemEntity = GetEntity(TransformUsageFlags.None);
            AddComponent(lootItemEntity, new LootItemEntity { PrefabEntity = prefabEntity });
        }
    }
}


///<returns>-LootIndexByType-    use globaldata.lootypes </returns>

public struct EntitiesAndIndexes
{
    public int LootType;
    public int LootIndexByType;
    public Entity LootEntity;
}

public struct LootController_Item_LootBufferData : IBufferElementData
{
    public EntitiesAndIndexes entitiesAndIndexes;
}

public struct LootController_XpLootBufferData : IBufferElementData
{
    public EntitiesAndIndexes entitiesAndIndexes;
}
public struct IEnterCollectionRange : IComponentData { }
public struct XpAmoutData : IComponentData { public int xp; }

public struct LootData : IComponentData
{
    public int LootType;
    public int EntityIndex;
}
public struct CollectTheLoot : IComponentData { }

public struct LootItemEntity : IComponentData { public Entity PrefabEntity; }

public struct LootItemChosenTag : IComponentData { }
public struct LootItemTag : IComponentData { public int ItemPossibleLootIndex; }
public struct LootItemData : IComponentData
{
    public int suffixeIndex;
    public int rarityIndex;
}

public struct LootPositionDataForManaged : IComponentData
{
    public int lootChanceForManaged;
    public float3 position;
}
public struct ReadyToAddXPData : IComponentData { public int entityIndex; }
public struct PlayAudio_DelegateCallerData : IComponentData { public bool leveledUp; }

[BurstCompile]
public partial struct CollectLoot : ISystem
{

    private Unity.Mathematics.Random _random;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _random = new Unity.Mathematics.Random((uint)Time.frameCount + 1);// 0 olursa hata verir minimum 1 olmalý
        state.RequireForUpdate<LootController_XpLootBufferData>();
        state.RequireForUpdate<PlayerData>();

    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        var playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
        var playerPosition = SystemAPI.GetComponent<LocalToWorld>(playerEntity).Position;
        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var xpAmoutData_Lookup = SystemAPI.GetComponentLookup<XpAmoutData>(true);


        foreach (var (lootPositionData, lootPositionData_Entity) in SystemAPI.Query<LootPositionData>().WithEntityAccess())
        {
            Entity selectedEntity;
            DynamicBuffer<LootController_XpLootBufferData> LootController_XpLootBufferData = SystemAPI.GetSingletonBuffer<LootController_XpLootBufferData>(true);
            int rand = _random.NextInt(0, LootController_XpLootBufferData.Length);
            int lootChance = _random.NextInt(0, 10);
            float size = 0.5f;
            selectedEntity = LootController_XpLootBufferData[rand].entitiesAndIndexes.LootEntity;
            switch (rand)
            {
                case 0: size = 0.8f; break;
                case 1: size = 0.9f; break;
                case 2: size = 0.95f; break;
                case 3: size = 1f; break;
                default: size = 1f; break;
            }
            var createdEntity = ecb.Instantiate(selectedEntity);

            if (lootChance == 1)
            {
                EntityQuery query = SystemAPI.QueryBuilder().WithAll<LootItemTag>().Build(); // Þartlarýna göre düzenle
                int count = query.CalculateEntityCount();
                Entity randomEntity = Entity.Null;
                if (count > 0)
                {
                    using NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
                    int randomIndex = _random.NextInt(count);
                    randomEntity = entities[randomIndex];
                }
                if (randomEntity.Equals(Entity.Null) == false)
                {
                    ecb.AddComponent(randomEntity, new LootItemChosenTag());
                    ecb.SetComponent(randomEntity, new LocalTransform
                    {
                        Position = new float3 { x = lootPositionData.destroyedMonsterPosition.x, y = lootPositionData.destroyedMonsterPosition.y + 0.5f, z = lootPositionData.destroyedMonsterPosition.z },
                        Rotation = quaternion.identity,
                        Scale = 2,
                    });
                }

            }

            ecb.AddComponent(createdEntity, new LootData { LootType = (int)LootController.LootTypes.XP, EntityIndex = rand });
            ecb.SetComponent(createdEntity, new LocalTransform
            {
                Position = lootPositionData.destroyedMonsterPosition,
                Rotation = quaternion.identity,
                Scale = size,
            });
            ecb.DestroyEntity(lootPositionData_Entity);
        }

        foreach (var (lootLocalTransform, lootEntity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<LootData>().WithNone<ReadyToAddXPData>().WithEntityAccess())
        {
            if (math.distance(playerPosition, lootLocalTransform.ValueRW.Position) < 5f)
            {
                if ((SystemAPI.GetComponent<LootData>(lootEntity).LootType == (int)LootController.LootTypes.XP))
                {
                    ecb.AddComponent<ReadyToAddXPData>(lootEntity, new ReadyToAddXPData { entityIndex = (SystemAPI.GetComponent<LootData>(lootEntity).EntityIndex) });
                }
                ecb.AddComponent(lootEntity, new IEnterCollectionRange());
                ecb.RemoveComponent<LootData>(lootEntity);
            }
        }
        foreach (var (lootLocalTransform, lootEntity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<IEnterCollectionRange>().WithNone<ReadyToAddXPData>().WithEntityAccess())
        {

            float3 direction = math.normalize(playerPosition - lootLocalTransform.ValueRW.Position);
            lootLocalTransform.ValueRW.Position += direction * 7.5f * deltaTime;

            if (math.distance(playerPosition, lootLocalTransform.ValueRW.Position) < 0.3f)
            {
                if (xpAmoutData_Lookup.HasComponent(lootEntity))
                {

                    PlayerData playerData = SystemAPI.GetSingleton<PlayerData>();
                    bool levelUp = false;
                    if (playerData.XpToNextLevel <= playerData.CurrentXP)
                    {

                        levelUp = true;
                    }
                    playerData.CurrentXP += xpAmoutData_Lookup.GetRefROOptional(lootEntity).ValueRO.xp;

                    var createdEntity = ecb.CreateEntity();
                    ecb.AddComponent(createdEntity, new PlayAudio_DelegateCallerData());
                    PlayAudio_DelegateCallerData playAudio_DelegateCallerData = new PlayAudio_DelegateCallerData { leveledUp = levelUp };
                    ecb.SetComponent(createdEntity, playAudio_DelegateCallerData);
                    ecb.SetComponent(playerEntity, playerData);

                }
                ecb.DestroyEntity(lootEntity);
            }

        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public partial class CollectLootManaged : SystemBase
{
    private bool WokedOneTime;
    protected override void OnUpdate()
    {

        if (WokedOneTime == false)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            if (SystemAPI.TryGetSingleton<LootItemEntity>(out LootItemEntity lootItemEntity))
            {
                if (LootController.Instance.finished == true)
                {
                    int indexx = 0;
                    foreach (var item in LootController.Instance.ChosenLootItemListForInventory)
                    {
                        
                        Entity entity =ecb.Instantiate(lootItemEntity.PrefabEntity);

                        ecb.AddComponent< LootItemData > (entity);
                        LootItemData lootItemData = new LootItemData { suffixeIndex = (int)item.suffixName, rarityIndex = (int)item.rarityName };
                        ecb.SetComponent< LootItemData>(entity, lootItemData);

                        ecb.AddComponent<LootItemTag>(entity);
                        LootItemTag lootItemTag = new LootItemTag { ItemPossibleLootIndex = indexx };
                        ecb.SetComponent<LootItemTag>(entity, lootItemTag);

                        indexx++;
                        WokedOneTime = true;
                    }
                }
            }
            ecb.Playback(World.EntityManager);
            ecb.Dispose();
        }

        if (SystemAPI.TryGetSingletonEntity<PlayerData>(out Entity playerEntity))
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            var playerPosition = SystemAPI.GetComponent<LocalToWorld>(playerEntity).Position;

            foreach (var (ReadyToAddXPData, lootEntity) in SystemAPI.Query<RefRW<ReadyToAddXPData>>().WithEntityAccess())
            {


                int Xp = (int)(math.pow(1.5f, (SystemAPI.GetComponent<ReadyToAddXPData>(lootEntity).entityIndex) + 1) * 5 * GlobalModifiersCalculator.GetModifiersValue(typeof(GlobalData.PlayerModifiers), (int)GlobalData.PlayerModifiers.Xp));
                ecb.AddComponent(lootEntity, new XpAmoutData { xp = Xp });

                ecb.AddComponent(lootEntity, new IEnterCollectionRange());
                ecb.RemoveComponent<ReadyToAddXPData>(lootEntity);

            }
            foreach (var (playAudio_DelegateCallerData, entity) in SystemAPI.Query<RefRW<PlayAudio_DelegateCallerData>>().WithEntityAccess())
            {
                PlayerData playerData = SystemAPI.GetSingleton<PlayerData>();
                if (playAudio_DelegateCallerData.ValueRO.leveledUp)
                {
                    playerData.level += 1;
                    playerData.CurrentXP = 0;
                    playerData.XpToNextLevel = (int)(PlayerLocation_Authorizer.BaseXpToLevel * math.pow(1.5, playerData.level));
                    LevelUpBuffController.Instance.SetFlippingCardObjects();
                }
                else
                {
                    AudioController.PlayAudio_Delegate?.Invoke((int)AudioController.SoundTypes.collectXP);
                }
                ecb.DestroyEntity(entity);
                ecb.SetComponent(playerEntity, playerData);
            }



            foreach (var (lootItemData, itemEntity) in SystemAPI.Query<LootItemData>().WithAll<LootItemChosenTag>().WithEntityAccess())
            {
                var renderer = SystemAPI.ManagedAPI.GetComponent<SpriteRenderer>(itemEntity);

                renderer.sprite = LootController.Instance.spritesByTypeAndRarity[lootItemData.suffixeIndex].rarityToSprite[lootItemData.rarityIndex];
                ecb.RemoveComponent<LootPositionDataForManaged>(itemEntity);
                ecb.RemoveComponent<LootItemChosenTag>(itemEntity);
                ecb.RemoveComponent<LootItemData>(itemEntity);
            }

            foreach (var (localTransform, lootItemTag, itemEntity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<LootItemTag>>().WithNone<LootItemData>().WithNone<LootItemChosenTag>().WithEntityAccess())
            {

                float3 direction = math.normalize(playerPosition - localTransform.ValueRW.Position);
                localTransform.ValueRW.Position += direction * 7.5f * SystemAPI.Time.DeltaTime;

                if (math.distance(playerPosition, localTransform.ValueRW.Position) < 0.3f)
                {
                    InventoryManager.Instance.CollectItem(lootItemTag.ValueRO.ItemPossibleLootIndex);
                    AudioController.PlayAudio_Delegate?.Invoke((int)AudioController.SoundTypes.collectItem);
                    ecb.DestroyEntity(itemEntity);
                }
            }

            ecb.Playback(World.DefaultGameObjectInjectionWorld.EntityManager);
            ecb.Dispose();
        }
    }

}