using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{

    public string Name;
    public string Description;
    public MonsterMainController.MonsterTypes MonsterTypes;
    public GlobalData.RarityDegree Rarity;
    public Texture monsterTexture;
    public int monsterTextureNumber; // Monster main controller Icon list number

    public float Scale =1;
    public float Speed = 2;
    public float SeparationRadius = 0.5f;

    public float HP;
    public float BaseMagicResistance;
    public float BaseDamage;

    public float DamageCooldown_Limit =1f;
    public float DamageCooldown =0f;

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

    public float TriggerSize;

    public bool IsMovementBlocked;

}
