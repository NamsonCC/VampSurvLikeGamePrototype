using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

#region Authorizer
public class MonsterSpawnable_Authorizer : MonoBehaviour
{

    public GameObject monsterPrefab;
    public int index;
    private class Baker : Baker<MonsterSpawnable_Authorizer>
    {
        private static int counter = 0;
        public override void Bake(MonsterSpawnable_Authorizer authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            Entity prefabEntity = GetEntity(authoring.monsterPrefab, TransformUsageFlags.Dynamic);
            authoring.index = counter;
            counter++;

            AddComponent(entity, new MonsterPrefab
            {
                PrefabEntity = prefabEntity,
                index = authoring.index,
            });
        }
    }
}

#region IcomponentData
public struct MonsterPrefab : IComponentData
{
    public Entity PrefabEntity;
    public int index;
}
public struct DisabledTag : IComponentData { }
#endregion
#endregion



