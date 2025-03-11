using UnityEngine;

[CreateAssetMenu(fileName = "CharactersScriptableObject", menuName = "Scriptable Objects/CharactersScriptableObject")]
public class CharactersScriptableObject : ScriptableObject
{
    public float HP;
    public float Speed;
    public float HP_Regeneration;
    public float BaseResistance;
    public float MagicResistance;
    public float PhysicalResistance;
    public float BaseDamage;

    public float BaseTakenDamageCooldown = 1f;
    public float timePassed;
}
