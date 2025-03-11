using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


#region PlayerMovementSystem_Isystems
public partial struct PlayerInputSystem : ISystem
{
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition; 
    private bool isTouching;
    public void OnUpdate(ref SystemState state)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    isTouching = true;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    currentTouchPosition = touch.position;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }
        if (isTouching == false)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            foreach (var input in SystemAPI.Query<RefRW<PlayerInputComponent>>())
            {
                input.ValueRW.inputDirection = new float2(horizontal, vertical);
            }
        }
        else
        {
            foreach (var input in SystemAPI.Query<RefRW<PlayerInputComponent>>())
            {
                input.ValueRW.inputDirection = GetNormalizedDirection();
            }
        }
    }

    public float2 GetNormalizedDirection()
    {
        if (isTouching)
        {
            float2 direction = new float2((currentTouchPosition.x - startTouchPosition.x), (currentTouchPosition.y - startTouchPosition.y));
            float2 pos = math.normalize(direction);
            if (float.IsNaN(pos.x) || float.IsNaN(pos.y))
            {
                return float2.zero; ;
            }
            return math.normalize(direction);

        }
        return float2.zero;
    }
}

public partial struct PlayerMovementSystem : ISystem
{
    LocalToWorld lclToWorld;

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        foreach (var (localTransform, worldTransform, movement, input, playerEntity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<LocalToWorld>, RefRO<PlayerMovementComponent>, RefRW<PlayerInputComponent>>().WithEntityAccess())
        {
            if (math.lengthsq(input.ValueRW.inputDirection) > 1e-5f)
            {
                input.ValueRW.inputDirection = math.normalize(input.ValueRW.inputDirection);
                var renderer = SystemAPI.ManagedAPI.GetComponent<SpriteRenderer>(playerEntity);
                renderer.material.SetFloat("_Idle", 0);

                if (input.ValueRW.inputDirection.x > 0)
                {
                    lclToWorld = worldTransform.ValueRW;
                    lclToWorld.Value.c0.x = -1 * localTransform.ValueRO.Scale;
                }
                else
                {
                    lclToWorld = worldTransform.ValueRW;
                    lclToWorld.Value.c0.x = 1 * localTransform.ValueRO.Scale;
                }
            }
            else
            {
                var renderer = SystemAPI.ManagedAPI.GetComponent<SpriteRenderer>(playerEntity);
                renderer.material.SetFloat("_Idle", 1);
            }
            if (!lclToWorld.Equals(default(LocalToWorld)))
            {
                worldTransform.ValueRW.Value = lclToWorld.Value;
            }

            float2 targetPosition = localTransform.ValueRO.Position.xy + input.ValueRO.inputDirection * movement.ValueRO.moveSpeed * deltaTime;
            float2 currentPosition = new float2(Mathf.Clamp(localTransform.ValueRO.Position.x, -80, 80), Mathf.Clamp(localTransform.ValueRO.Position.y, -80, 80));
            localTransform.ValueRW.Position.xy = math.lerp(currentPosition, targetPosition, 0.75f);
        }
    }
}

#endregion
#region PlayerLocAuthorizer
public class PlayerLocation_Authorizer : MonoBehaviour
{
    /// There will be lots of playable characters
    public CharactersScriptableObject scriptableObject_Player;
    public float moveSpeed = 5f;
    public static int BaseXpToLevel = 100;
    public Animator animator;
    class Baker : Baker<PlayerLocation_Authorizer>
    {

        public ScriptableObject scriptableObject_Player;
        public override void Bake(PlayerLocation_Authorizer authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerMovementComponent { moveSpeed = authoring.moveSpeed });
            AddComponent(entity, new PlayerTag());

            AddComponent(entity, new PlayerInputComponent());
            AddComponent(entity, new PlayerData
            {
                HP = authoring.scriptableObject_Player.HP,
                BaseHP = authoring.scriptableObject_Player.HP,
                Speed = authoring.scriptableObject_Player.Speed,
                HP_Regeneration = authoring.scriptableObject_Player.HP_Regeneration,
                BaseResistance = authoring.scriptableObject_Player.BaseResistance,
                MagicResistance = authoring.scriptableObject_Player.MagicResistance,
                PhysicalResistance = authoring.scriptableObject_Player.PhysicalResistance,
                BaseDamage = authoring.scriptableObject_Player.BaseDamage,
                BaseTakenDamageCooldown = authoring.scriptableObject_Player.BaseTakenDamageCooldown,
                timePassed = authoring.scriptableObject_Player.timePassed,
                entity = entity,
                level = 1,
                XpToNextLevel = (int)(BaseXpToLevel * math.pow(1.5, 1))
            });

        }
    }
}

#region IcomponentData
public struct PlayerData : IComponentData
{
    public float HP;
    public float BaseHP;
    public float Speed;
    public float HP_Regeneration;
    public float BaseResistance;
    public float MagicResistance;
    public float PhysicalResistance;
    public float BaseDamage;

    public float BaseTakenDamageCooldown;
    public float timePassed;

    public Entity entity;

    public int level;
    public int CurrentXP;
    public int XpToNextLevel;
}

public struct PlayerMovementComponent : IComponentData
{
    public float moveSpeed;
}


public struct PlayerInputComponent : IComponentData
{
    public float2 inputDirection;
}
public struct PlayerTag : IComponentData { }



#endregion
#endregion

