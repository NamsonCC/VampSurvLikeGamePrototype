using Unity.Physics;
using UnityEngine;

[CreateAssetMenu(fileName = "Bolt Projectile Base", menuName = "Scriptable Objects/Bolt Projectile Base")]
public class BoltProjectileBase : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public int IconNumber; // Monster main controller Icon list number


    public float BoltSpeed = 5f;
    public float BoltScale = 1f;
    public float BoltDamage = 10;
    public float CoolDown =5f;
    public float TimePassed_AfterLastCast;


    public MagicalDamageData magicalDamageData;



    public CollisionFilter collisionFilter;
}

[System.Serializable]
public struct MagicalDamageData
{
   public GlobalData.MagicalDamageType damageType;
    


    public CriticalChanceData criticalChanceData;
    public PierceData PierceData;
}
[System.Serializable]
public struct CriticalChanceData
{
    public bool IsCriticalHitPossible;  
    public float CriticalHitChance; 
}
[System.Serializable]
public struct PierceData
{
    public float MagicalPiercingPercantage;
    public bool CanPierce_AfterittingEnemy;
    public float HowManyTimesCanPierceEnemy;
}