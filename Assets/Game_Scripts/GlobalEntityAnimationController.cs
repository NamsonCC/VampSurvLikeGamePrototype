using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

public class GlobalEntityAnimationController : MonoBehaviour
{
    public static GlobalEntityAnimationController globalEntityAnimationController;
    private void Awake()
    {
        globalEntityAnimationController = this;
    }
}
[BurstCompile]
public partial struct GlobalAnimationSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {

    }
    public void OnDestroy(ref SystemState state)
    {

    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var job = new ScaleAndRotationJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        job.ScheduleParallel();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
[BurstCompile]
public partial struct ScaleAndRotationJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(ref LocalToWorld localToWorld, ref ScaleData scaleData, ref RotationData rotationData)
    {
        scaleData.Time += DeltaTime * scaleData.Speed;
        float scaleFactor = math.sin(scaleData.Time); 
        float scaleX = math.lerp(scaleData.MinScaleX, scaleData.MaxScaleX, (scaleFactor + 1) * 0.5f);
        float3 scale = new float3(scaleX, scaleData.MaxScaleX, scaleData.MaxScaleX); 

        rotationData.Time += DeltaTime * rotationData.Speed;
        float rotationAngle = math.sin(rotationData.Time) * rotationData.Angle;
        quaternion rotation = quaternion.Euler(0, 0, math.radians(rotationAngle)); 


        localToWorld.Value = float4x4.TRS(
            localToWorld.Position, 
            rotation,              
            scale                  
        );
    }
}