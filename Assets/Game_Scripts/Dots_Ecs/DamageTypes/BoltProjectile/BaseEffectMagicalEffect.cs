using UnityEngine;

[CreateAssetMenu(fileName = "BaseEffectMagicalEffect", menuName = "Scriptable Objects/BaseEffectMagicalEffect")]
public class BaseEffectMagicalEffect : ScriptableObject
{
    [InspectorName("Bolt Projectile Base")]
    public BoltProjectileBase boltProjectileBase;

    [InspectorName("Type of effect")]
    public GlobalData.MagicalDamageType effectType;

    [InspectorName("Is Effect Active?")]
    public bool EffectActive;

    [InspectorName("Effect Damage")]
    public float EffectDamage;

    [InspectorName("Effect Duration")]
    public float Duration;

    public GameObject ProjecctileEffect;
}
