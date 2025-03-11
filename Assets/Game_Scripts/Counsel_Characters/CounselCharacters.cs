using UnityEngine;
[CreateAssetMenu(fileName = "CounselCharacters", menuName = "Scriptable Objects/CounselCharacters")]
public class CounselCharacters : ScriptableObject
{
    public string Name;
    public string Description;
    public GlobalData.RarityDegree Rarity;
    public Sprite Icon;

    public int RenownPrice;

    // Skill & Training Group
    public bool ShowSkillGroup;
    public int MaxSkillLevel;
    public int TrainingBonusPercantage;
    public int PossibleLootPercantage;
    public int PossibleLootQuality;

    // Enemy Weakness Group
    public bool ShowWeaknessGroup;
    public int ReduceEnemyResistance;
    public int ReduceEnemyMagicResistence;
    public int CriticalWeakness;
    public int FireWeakness;
    public int FrostWeakness;
    public int ShockWeakness;

    // Effect Strength Group
    public bool ShowEffectGroup;
    public int IncreaseDamageEffectStrenght;
    public int IncreaseBleedEffectStrenght;
    public int IncreasePoisonEffectStrenght;
    public int IncreaseFrostEffectStrenght;
    public int IncreaseFireEffectStrenght;
    public int IncreaseShockEffectStrenght;

    // Enemy Modifier Group
    public bool ShowModifierGroup;
    public int ReduceEnemyPhysicalResistence;
    public int ReduceEnemyHp;
    public int ReduceEnemySpeed;

    // Other Modifiers
    public bool ShowOtherGroup;
    public int ReduceEnemyRegeneration;
    public int IncreaseEnemyCooldown;
    public int ReviveAgain;
}
