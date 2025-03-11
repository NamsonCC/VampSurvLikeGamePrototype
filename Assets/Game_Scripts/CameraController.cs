using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance;
    public Image PlayerHealthBar;

    public Entity playerEntity;
    public EntityManager entityManager;
    public World world;
    public float3 offset;
    public float smoothSpeed = 5f;
    private void Awake()
    {
        Instance = this;
        SetPixelCamerasRes(Screen.width, Screen.height);
    }

    private void SetPixelCamerasRes(int x, int y)
    {
        PixelPerfectCamera pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        pixelPerfectCamera.refResolutionX = x;
        pixelPerfectCamera.refResolutionY = y;
    }
    private void Start()
    {
        MainMenuController.resplutionChangedDelegate += SetPixelCamerasRes;
    }
    private void OnDisable()
    {
        MainMenuController.resplutionChangedDelegate -= SetPixelCamerasRes;
    }
}

public partial class CameraMoveSystem : SystemBase
{
    public Entity playerEntity;
    float3 offset;
    float smoothSpeed;
    bool getValues;
    protected override void OnCreate()
    {
        base.OnCreate();
        getValues = true;

    }
    protected override void OnUpdate()
    {
        if (CameraController.Instance ==null)
        {
            return;
        }
        if (getValues)
        {
            offset = CameraController.Instance.offset;
            smoothSpeed = CameraController.Instance.smoothSpeed;
            getValues = false;
        }

        SystemAPI.TryGetSingletonEntity<PlayerMovementComponent>(out Entity entity);
        playerEntity = entity;
        if (playerEntity != null)
        {
            var entityManager = World.EntityManager;
            if (SystemAPI.TryGetSingleton<PlayerData>(out var playerDataComponent))
            {
                CameraController.Instance.PlayerHealthBar.fillAmount = playerDataComponent.HP/ playerDataComponent.BaseHP;
                float3 pos = SystemAPI.GetComponentRO<LocalToWorld>(playerEntity).ValueRO.Position;
                pos.y -= 0.65f;
                CameraController.Instance.PlayerHealthBar.gameObject.transform.position = pos;
            }

            if (entityManager.Exists(playerEntity))
            {

                var playerPosition = entityManager.GetComponentData<Unity.Transforms.LocalTransform>(playerEntity).Position;
                Vector3 targetPosition = playerPosition + offset;
                Transform transfr = CameraController.Instance.gameObject.transform;
                transfr.position = Vector3.Lerp(transfr.position, targetPosition, smoothSpeed * SystemAPI.Time.DeltaTime);

            }
        }
        else
        {
            Debug.Log("playerEntity");
        }
    }
}