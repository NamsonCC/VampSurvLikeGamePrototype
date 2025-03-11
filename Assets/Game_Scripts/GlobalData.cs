using System;
using System.Collections.Generic;
using Unity.Physics;
using UnityEditor;
using UnityEngine;

public class GlobalData : MonoBehaviour
{

    public static GlobalData Instance;

    public enum RarityDegree
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical,
        Divine
    }
    public enum RarityColors
    {
        Orange,
        Green,
        Blue,
        Violet,
        Golden,
        Red,
        White_LightBlue,
    }

    public enum RangedProjectileTypes
    {
        Normal,            // Standart davran��, do�rudan hedefe gider.
        Bouncing,          // Sekme, duvarlara veya zeminlere �arpt���nda seken mermi.
        Explosive,         // Patlayan m�himmat, hedefe �arpt���nda patlar.
        Piercing,          // Delici m�himmat, hedefin i�ine girer ve ge�er.
        Splitting,         // Birden fazla mermiye ayr�lan m�himmat (�rne�in, b�l�nerek yay�l�r).
        Ricochet,          // Bir sefer sekip sonra durma, yava��a giden bir sekme davran���.
        Frozing,               // 
        Vortex,            // Hedefi bir araya �eken bir t�r patlama, her �eyi ortas�nda toplar.
        Splitter,          // Bir mermi hedefe �arpt���nda b�l�nerek yeni mermilere d�n���r.
        ChainReaction,     // Hedefe �arpan mermi, patlamalar zinciri ba�lat�r.
        Shrapnel,          // Hedefe �arpt���nda par�alanarak etrafa yay�l�r.
    }

    /// <summary>
    /// All of them are Magical Damage
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    ///     <item>Standard_Index</item>
    ///     <item>Bleed_Index</item>
    ///     <item>Poison_Index</item>
    ///     <item>Burn_Index</item>
    ///     <item>Froze_Index</item>
    ///     <item>Shock_Index</item>
    /// </list>
    /// </returns>
    public enum MagicalDamageType
    {
        Standard,
        Bleed,
        Poison,
        Burn,
        Freeze,
        Shock
    }

    /// <returns>
    /// <list type="bullet">
    ///     <item>BoltSpeed_Index</item>
    ///     <item>BoltDamage_Index</item>
    ///     <item>BoltColliderScale_Index</item>
    ///     <item>CoolDown_Index</item>
    ///     <item>PhysicalPiercingPercentage_Index</item>
    ///     <item>MagicalPiercingPercentage_Index</item>
    ///     <item>HowManyTimesCanPierceEnemy_Index</item>
    /// 
    /// </list>
    /// </returns>
    public enum ProjectileModifiers
    {
        BoltSpeed,
        BoltDamage,
        ExplosionDamage,
        BoltColliderScale,
        CoolDown,
        MagicalPiercingPercentage,
        HowManyTimesCanPierceEnemy,
    }
    private enum CollisionLayer
    {
        Solid = 1 << 0,
        Character = 1 << 1,
        Monster = 1 << 2,
        Projectile = 1 << 3,
        Loot = 1 << 4
    }

    public static CollisionFilter
        FilterSolid = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Solid),
            CollidesWith = (uint)(CollisionLayer.Character)
        },
        FilterCharacter = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Character),
            CollidesWith = (uint)(CollisionLayer.Solid | CollisionLayer.Loot | CollisionLayer.Monster)
        },
        FilterMonster = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Monster),
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Projectile | CollisionLayer.Monster)
        },
        FilterProjectile = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Projectile),
            CollidesWith = (uint)(CollisionLayer.Monster)
        },
        FilterItemLoot = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Loot),
            CollidesWith = (uint)(CollisionLayer.Character)
        };

    /// <returns>
    /// <list type="bullet">
    ///     <item>allDamageTypes</item>
    ///     <item>critical</item>
    /// </list>
    /// </returns>
    public enum DamageModifiers
    {
        allDamageTypes,
        critical,
    }
    public enum PlayerModifiers
    {
        Speed,
        Hp,
        Xp,
        CollectionRange,
        Resistance,
        Luck,
    }
    public enum LootType
    {
        Item,
        CraftingItem,
    }

    public Dictionary<RarityDegree, RarityColors> Rarity_ColorPairs = new Dictionary<RarityDegree, RarityColors>();

    public static float GameStartedTimer;
    private void Start()
    {
        Instance = this;
        int index = 0;
        foreach (var item in Enum.GetValues(typeof(RarityDegree)))
        {
            Rarity_ColorPairs.Add((RarityDegree)index, (RarityColors)index);
            index++;
        }
    }
}
