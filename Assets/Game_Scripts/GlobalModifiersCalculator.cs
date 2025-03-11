using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalModifiersCalculator : MonoBehaviour
{
    #region InitializedValues
    public static Dictionary<GlobalData.ProjectileModifiers, float> Projectile_ModifiersDictionary = new Dictionary<GlobalData.ProjectileModifiers, float>();
    public static Dictionary<GlobalData.MagicalDamageType, float> DamageType_Dictionary = new Dictionary<GlobalData.MagicalDamageType, float>();
    public static Dictionary<GlobalData.DamageModifiers, float> Damage_ModifiersDictionary = new Dictionary<GlobalData.DamageModifiers, float>();
    public static Dictionary<GlobalData.PlayerModifiers, float> Player_ModifiersDictionary = new Dictionary<GlobalData.PlayerModifiers, float>();

    public static Dictionary<object, float> ModifiersFromItem_Dictionary = new Dictionary<object, float>();
    public static Dictionary<object, float> ModifiersFromLevelUp_Dictionary = new Dictionary<object, float>();

    public static NativeHashMap<int, float> Projectile_ModifiersHashMap;
    public static NativeHashMap<int, float> Damage_ModifiersHashMap;
    public static NativeHashMap<int, float> DamageType_ModifiersHashMap;
    public static NativeHashMap<int, float> Player_ModifiersHashMap;


    private static bool _initialized;
    private bool reseted;

    public static Action LevelUp_DictionaryChanged_Action;
    private void OnApplicationQuit()
    {
        ResetValues();

    }
    private void OnDestroy()
    {
        ResetValues();
    }

    private void Start()
    {
        if (_initialized == false)
        {
            InitializeModifiers();
        }
        SceneManager.sceneLoaded += CallResetValues;
    }
    private void CallResetValues(Scene scene, LoadSceneMode mode)
    {
        ResetValues();
    }
    private void ResetValues()
    {
        LevelUp_DictionaryChanged_Action = null;
        try
        {
            if (Projectile_ModifiersHashMap.IsCreated)
                Projectile_ModifiersHashMap.Dispose();
        }
        finally
        {
            if (Damage_ModifiersHashMap.IsCreated)
                Damage_ModifiersHashMap.Dispose();
        }

        try
        {
            if (DamageType_ModifiersHashMap.IsCreated)
                DamageType_ModifiersHashMap.Dispose();
        }
        finally
        {
            if (Player_ModifiersHashMap.IsCreated)
                Player_ModifiersHashMap.Dispose();
        }

        Projectile_ModifiersDictionary.Clear();
        DamageType_Dictionary.Clear();
        Damage_ModifiersDictionary.Clear();
        Player_ModifiersDictionary.Clear();
        ModifiersFromItem_Dictionary.Clear();
        ModifiersFromLevelUp_Dictionary.Clear();
        _initialized = false;

    }

    #region Initilize
    public static void InitializaHashMaps()
    {

        if (Projectile_ModifiersHashMap.IsCreated)
            Projectile_ModifiersHashMap.Dispose();
        if (DamageType_ModifiersHashMap.IsCreated)
            DamageType_ModifiersHashMap.Dispose();
        if (Damage_ModifiersHashMap.IsCreated)
            Damage_ModifiersHashMap.Dispose();
        if (Player_ModifiersHashMap.IsCreated)
            Player_ModifiersHashMap.Dispose();




        // Initialize NativeHashMap with the correct capacity
        Projectile_ModifiersHashMap = new NativeHashMap<int, float>(Projectile_ModifiersDictionary.Count, Allocator.Persistent);
        DamageType_ModifiersHashMap = new NativeHashMap<int, float>(DamageType_Dictionary.Count, Allocator.Persistent);
        Damage_ModifiersHashMap = new NativeHashMap<int, float>(Damage_ModifiersDictionary.Count, Allocator.Persistent);
        Player_ModifiersHashMap = new NativeHashMap<int, float>(Player_ModifiersDictionary.Count, Allocator.Persistent);


        foreach (var item in Projectile_ModifiersDictionary)
        {
            int key = (int)item.Key;
            Projectile_ModifiersHashMap.TryAdd(key, item.Value);
        }
        foreach (var item in DamageType_Dictionary)
        {
            int key = (int)item.Key;
            DamageType_ModifiersHashMap.TryAdd(key, item.Value);

        }
        foreach (var item in Damage_ModifiersDictionary)
        {
            int key = (int)item.Key;
            Damage_ModifiersHashMap.TryAdd(key, item.Value);
        }
        foreach (var item in Player_ModifiersDictionary)
        {
            int key = (int)item.Key;
            Player_ModifiersHashMap.TryAdd(key, item.Value);
        }
    }

    public static void InitializeModifiers()
    {




        if (_initialized) return;
        foreach (GlobalData.ProjectileModifiers modifier in Enum.GetValues(typeof(GlobalData.ProjectileModifiers)))
        {
            float defaultValue = GetDefaultValueForModifier_ProjectileModifiers(modifier);
            Projectile_ModifiersDictionary.Add(modifier, defaultValue);
        }
        foreach (GlobalData.MagicalDamageType modifier in Enum.GetValues(typeof(GlobalData.MagicalDamageType)))
        {
            float defaultValue = GetDefaultValueForModifier_DamageType(modifier);
            DamageType_Dictionary.Add(modifier, defaultValue);
        }
        foreach (GlobalData.DamageModifiers modifier in Enum.GetValues(typeof(GlobalData.DamageModifiers)))
        {
            float defaultValue = GetDefaultValueForModifier_DamageModifiers(modifier);
            Damage_ModifiersDictionary.Add(modifier, defaultValue);
        }
        foreach (GlobalData.PlayerModifiers playerModifiers in Enum.GetValues(typeof(GlobalData.PlayerModifiers)))
        {
            float defaultValue = GetDefaultValueForModifier_PlayerModifiers(playerModifiers);
            Player_ModifiersDictionary.Add(playerModifiers, defaultValue);
        }

        foreach (GlobalData.ProjectileModifiers modifier in Enum.GetValues(typeof(GlobalData.ProjectileModifiers)))
        {
            ModifiersFromItem_Dictionary.Add(modifier.ToString(), 0);
            ModifiersFromLevelUp_Dictionary.Add(modifier.ToString(), 0);
        }
        foreach (GlobalData.MagicalDamageType modifier in Enum.GetValues(typeof(GlobalData.MagicalDamageType)))
        {
            ModifiersFromItem_Dictionary.Add(modifier.ToString(), 0);
            ModifiersFromLevelUp_Dictionary.Add(modifier.ToString(), 0);
        }
        foreach (GlobalData.DamageModifiers modifier in Enum.GetValues(typeof(GlobalData.DamageModifiers)))
        {
            ModifiersFromItem_Dictionary.Add(modifier.ToString(), 0);
            ModifiersFromLevelUp_Dictionary.Add(modifier.ToString(), 0);
        }
        foreach (GlobalData.PlayerModifiers modifier in Enum.GetValues(typeof(GlobalData.PlayerModifiers)))
        {
            ModifiersFromItem_Dictionary.Add(modifier.ToString(), 0);
            ModifiersFromLevelUp_Dictionary.Add(modifier.ToString(), 0);
        }

        InitializaHashMaps();
        _initialized = true;
    }


    private static float GetDefaultValueForModifier_ProjectileModifiers(GlobalData.ProjectileModifiers modifier)
    {
        switch (modifier)
        {
            case GlobalData.ProjectileModifiers.BoltSpeed:
                return 1f; // Örnek varsayýlan deðer
            case GlobalData.ProjectileModifiers.BoltDamage:
                return 1f;
            case GlobalData.ProjectileModifiers.ExplosionDamage:
                return 1f;
            case GlobalData.ProjectileModifiers.BoltColliderScale:
                return 1f;
            case GlobalData.ProjectileModifiers.CoolDown:
                return 1f;
            case GlobalData.ProjectileModifiers.MagicalPiercingPercentage:
                return 0f;
            case GlobalData.ProjectileModifiers.HowManyTimesCanPierceEnemy:
                return 1f;
            default:
                throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
        }
    }

    private static float GetDefaultValueForModifier_DamageType(GlobalData.MagicalDamageType modifier)
    {
        //Base damage for all type
        return 1f;
    }


    private static float GetDefaultValueForModifier_DamageModifiers(GlobalData.DamageModifiers modifier)
    {
        switch (modifier)
        {
            case GlobalData.DamageModifiers.allDamageTypes:
                return 1f;
            case GlobalData.DamageModifiers.critical:
                return 2f;
            default:
                return 1f;
        }
    }

    private static float GetDefaultValueForModifier_PlayerModifiers(GlobalData.PlayerModifiers modifier)
    {
        switch (modifier)
        {
            case GlobalData.PlayerModifiers.Speed:
                return 1f;
            case GlobalData.PlayerModifiers.Hp:
                return 1f;
            case GlobalData.PlayerModifiers.Xp:
                return 1f;
            case GlobalData.PlayerModifiers.CollectionRange:
                return 1f;
            case GlobalData.PlayerModifiers.Resistance:
                return 1f;
            case GlobalData.PlayerModifiers.Luck:
                return 1f;
            default: return 1f;
        }
    }

    #endregion 
    #endregion
    #region Calculate
    public static T Calculate<TEnum, T>(TEnum keyEnum, T inputValue, Func<float, T, T> calculationFunc)
    {
        if (_initialized == false)
        {
            InitializeModifiers();
        }


        if (inputValue is not float && inputValue is not int)
            UnityEngine.Debug.LogError("Only int and float are supported.");



        if (keyEnum.GetType() == typeof(GlobalData.ProjectileModifiers))
        {
            if (!Projectile_ModifiersDictionary.ContainsKey((GlobalData.ProjectileModifiers)(object)keyEnum))
            {
                UnityEngine.Debug.LogError($"'{keyEnum}' anahtarý için bir modifikasyon bulunamadý.");
            }
            float modifier = Projectile_ModifiersDictionary[(GlobalData.ProjectileModifiers)(object)keyEnum];
            return calculationFunc(modifier, inputValue);
        }
        else
        if (keyEnum.GetType() == typeof(GlobalData.MagicalDamageType))
        {
            if (!DamageType_Dictionary.ContainsKey((GlobalData.MagicalDamageType)(object)keyEnum))
            {
                UnityEngine.Debug.LogError($"'{keyEnum}' anahtarý için bir modifikasyon bulunamadý.");
            }
            float modifier = DamageType_Dictionary[(GlobalData.MagicalDamageType)(object)keyEnum];
            return calculationFunc(modifier, inputValue);
        }
        else
        if (keyEnum.GetType() == typeof(GlobalData.DamageModifiers))
        {
            if (!Damage_ModifiersDictionary.ContainsKey((GlobalData.DamageModifiers)(object)keyEnum))
            {
                UnityEngine.Debug.LogError($"'{keyEnum}' anahtarý için bir modifikasyon bulunamadý.");
            }
            float modifier = Damage_ModifiersDictionary[(GlobalData.DamageModifiers)(object)keyEnum];
            return calculationFunc(modifier, inputValue);
        }
        if (keyEnum.GetType() == typeof(GlobalData.PlayerModifiers))
        {
            if (!Player_ModifiersDictionary.ContainsKey((GlobalData.PlayerModifiers)(object)keyEnum))
            {
                UnityEngine.Debug.LogError($"'{keyEnum}' anahtarý için bir modifikasyon bulunamadý.");
            }
            float modifier = Player_ModifiersDictionary[(GlobalData.PlayerModifiers)(object)keyEnum];
            return calculationFunc(modifier, inputValue);
        }
        UnityEngine.Debug.LogError($"default(T) returned");
        return default(T);
    }

    public static float DamageCalculator<TEnum>(TEnum enumType, float damageAmount, bool add)
    {
        return Calculate(enumType, damageAmount, (modifier, value) =>
        {
            if (add)
            {
                return modifier + value;
            }
            else
            {
                return modifier * value;
            }
        });
    }

    #endregion
    #region SetDicitonaries
    public static void SetItemModifiersDictionary(Item item, bool add)
    {
        string std = "";
        foreach (var (modifier, value) in item.Modifiers)
        {

            if (add)
            {
                ModifiersFromItem_Dictionary[modifier] += (float)value;
                std += $"{modifier.ToString()} +={value.ToString()}, total {modifier.ToString()} {ModifiersFromItem_Dictionary[modifier]} -:::-";
            }
            else
            {
                ModifiersFromItem_Dictionary[modifier] -= (float)value;
                std += $"{modifier.ToString()} -={value.ToString()}, total {modifier.ToString()} {ModifiersFromItem_Dictionary[modifier]} -:::-";
            }

        }
        Debug.Log("Equiped Item modifiers: " + std);


    }
    public static void SetLevelUpModifiersDictionary(object modifier, float value)
    {
        ModifiersFromLevelUp_Dictionary[modifier] += value;
        Debug.Log($" Level Up modifiers:: {modifier.ToString()} -={value.ToString()}, total {modifier.ToString()} {ModifiersFromLevelUp_Dictionary[modifier]} -:::-");
        LevelUp_DictionaryChanged_Action?.Invoke();


    }
    #endregion



    #region GetValues

    /// <summary>
    /// Retrieves the modified modifiers for a specific key enum type.
    /// </summary>
    /// <returns>
    /// A dictionary of the modified modifiers for the specified enum type.
    /// <list type="bullet">
    ///     <item>Damage_ModifiersDictionary</item>
    ///     <item>Projectile_ModifiersDictionary</item>
    ///     <item>DamageType_Dictionary</item>
    /// </list>
    /// </returns>

    public static void GetModifiersHashMap(Type enumKey, out NativeHashMap<int, float> hashMap)
    {
        if (_initialized == false)
        {
            InitializeModifiers();
        }

        if (enumKey == typeof(GlobalData.ProjectileModifiers))
        {
            NativeHashMap<int, float> mergedHashMap = new NativeHashMap<int, float>(Projectile_ModifiersHashMap.Count, Allocator.Persistent);

            // Projectile_ModifiersHashMap içindeki verileri ekle
            foreach (var kvp in Projectile_ModifiersHashMap)
            {
                mergedHashMap[kvp.Key] = kvp.Value;
            }

            // ModifiersFromItem_Dictionary'deki verileri ekle
            foreach (var key in ModifiersFromItem_Dictionary.Keys)
            {
                if (Enum.TryParse(key.ToString(), out GlobalData.ProjectileModifiers modifier))
                {
                    int modifierKey = (int)modifier;
                    float value = ModifiersFromItem_Dictionary[key];

                    if (mergedHashMap.ContainsKey(modifierKey))
                    {
                        mergedHashMap[modifierKey] += value;
                    }
                    else
                    {

                        mergedHashMap[modifierKey] = value;
                    }
                }
            }

            foreach (var key in ModifiersFromLevelUp_Dictionary.Keys)
            {
                if (Enum.TryParse(key.ToString(), out GlobalData.ProjectileModifiers modifier))
                {
                    int modifierKey = (int)modifier;
                    float value = ModifiersFromLevelUp_Dictionary[key];

                    if (mergedHashMap.ContainsKey(modifierKey))
                    {

                        mergedHashMap[modifierKey] += value;

                    }
                    else
                    {
                        mergedHashMap[modifierKey] = value;
                    }
                }
            }

            hashMap = mergedHashMap;
            return;
        }
        else if (enumKey == typeof(GlobalData.DamageModifiers))
        {

            NativeHashMap<int, float> mergedHashMap = new NativeHashMap<int, float>(Damage_ModifiersHashMap.Count, Allocator.Persistent);

            // Projectile_ModifiersHashMap içindeki verileri ekle
            foreach (var kvp in Damage_ModifiersHashMap)
            {
                mergedHashMap[kvp.Key] = kvp.Value;
            }


            // ModifiersFromItem_Dictionary'deki verileri ekle
            foreach (var key in ModifiersFromItem_Dictionary.Keys)
            {
                if (Enum.TryParse(key.ToString(), out GlobalData.DamageModifiers modifier))
                {
                    int modifierKey = (int)modifier;
                    float value = ModifiersFromItem_Dictionary[key];

                    if (mergedHashMap.ContainsKey(modifierKey))
                    {
                        mergedHashMap[modifierKey] += value;
                    }
                    else
                    {
                        mergedHashMap[modifierKey] = value;
                    }
                }
            }
            foreach (var key in ModifiersFromLevelUp_Dictionary.Keys)
            {
                if (Enum.TryParse(key.ToString(), out GlobalData.DamageModifiers modifier))
                {
                    int modifierKey = (int)modifier;
                    float value = ModifiersFromLevelUp_Dictionary[key];

                    if (mergedHashMap.ContainsKey(modifierKey))
                    {
                        mergedHashMap[modifierKey] += value;
                    }
                    else
                    {
                        mergedHashMap[modifierKey] = value;
                    }
                }
            }

            hashMap = mergedHashMap;
            return;
        }
        else if (enumKey == typeof(GlobalData.MagicalDamageType))
        {

            NativeHashMap<int, float> mergedHashMap = new NativeHashMap<int, float>(DamageType_ModifiersHashMap.Count, Allocator.Persistent);

            foreach (var kvp in DamageType_ModifiersHashMap)
            {
                mergedHashMap[kvp.Key] = kvp.Value;
            }


            foreach (var key in ModifiersFromItem_Dictionary.Keys)
            {
                if (Enum.TryParse(key.ToString(), out GlobalData.MagicalDamageType modifier))
                {
                    int modifierKey = (int)modifier;
                    float value = ModifiersFromItem_Dictionary[key];

                    if (mergedHashMap.ContainsKey(modifierKey))
                    {
                        mergedHashMap[modifierKey] += value;
                    }
                    else
                    {
                        mergedHashMap[modifierKey] = value;
                    }
                }
            }
            foreach (var key in ModifiersFromLevelUp_Dictionary.Keys)
            {
                if (Enum.TryParse(key.ToString(), out GlobalData.MagicalDamageType modifier))
                {
                    int modifierKey = (int)modifier;
                    float value = ModifiersFromLevelUp_Dictionary[key];

                    if (mergedHashMap.ContainsKey(modifierKey))
                    {
                        mergedHashMap[modifierKey] += value;
                    }
                    else
                    {
                        mergedHashMap[modifierKey] = value;
                    }
                }
            }
            hashMap = mergedHashMap;
            return;
        }
        else if (enumKey == typeof(GlobalData.PlayerModifiers))
        {
            NativeHashMap<int, float> mergedHashMap = new NativeHashMap<int, float>(Player_ModifiersHashMap.Count, Allocator.Persistent);


            foreach (var kvp in Player_ModifiersHashMap)
            {
                mergedHashMap[kvp.Key] = kvp.Value;
            }

            foreach (var key in ModifiersFromItem_Dictionary.Keys)
            {
                if (Enum.TryParse(key.ToString(), out GlobalData.PlayerModifiers modifier))
                {
                    int modifierKey = (int)modifier;
                    float value = ModifiersFromItem_Dictionary[key];

                    if (mergedHashMap.ContainsKey(modifierKey))
                    {
                        mergedHashMap[modifierKey] += value;
                    }
                    else
                    {
                        mergedHashMap[modifierKey] = value;
                    }
                }
            }
            foreach (var key in ModifiersFromLevelUp_Dictionary.Keys)
            {
                if (Enum.TryParse(key.ToString(), out GlobalData.PlayerModifiers modifier))
                {
                    int modifierKey = (int)modifier;
                    float value = ModifiersFromLevelUp_Dictionary[key];

                    if (mergedHashMap.ContainsKey(modifierKey))
                    {
                        mergedHashMap[modifierKey] += value;
                    }
                    else
                    {
                        mergedHashMap[modifierKey] = value;
                    }
                }
            }



            hashMap = mergedHashMap;
            return;
        }


        throw new InvalidOperationException($"Unsupported enum type: {enumKey.GetType()}");
    }

    public static float GetModifiersValue(Type enumKey, int enumIndex)
    {
        if (_initialized == false)
        {
            InitializeModifiers();
        }
        if (enumKey == typeof(GlobalData.ProjectileModifiers))
        {
            return Projectile_ModifiersHashMap[enumIndex];
        }
        else if (enumKey == typeof(GlobalData.DamageModifiers))
        {
            return Damage_ModifiersHashMap[enumIndex];
        }
        else if (enumKey == typeof(GlobalData.MagicalDamageType))
        {
            return DamageType_ModifiersHashMap[enumIndex];
        }
        else if (enumKey == typeof(GlobalData.PlayerModifiers))
        {
            return Player_ModifiersHashMap[enumIndex];
        }

        throw new InvalidOperationException($"Unsupported enum type: {enumKey.GetType()}");
    }


    #endregion
}




