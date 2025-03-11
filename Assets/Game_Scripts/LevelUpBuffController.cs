using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpBuffController : MonoBehaviour
{

    #region Declerations
    [SerializeField] private int BuffCount = 3;
    [SerializeField] private int ModifierMultp = 2;

    [SerializeField] private GameObject _backgroundImage;
    [SerializeField] private GameObject[] _cardPositions;
    [SerializeField] private List<GameObject> InstatiatedCards = new List<GameObject>();
    [SerializeField] private GameObject _flippingSword_CardCanvasPrefabs;
    [SerializeField] private GameObject _flippingHearth_CardCanvasPrefabs;
    [SerializeField] private GameObject _flippingBook_CardCanvasPrefabs;
    [SerializeField] private GameObject _flippingArmor_CardCanvasPrefabs;



    private List<Item> itemList = new List<Item>();
    private Dictionary<GlobalData.RarityDegree, float> BuffRarity_Value = new Dictionary<GlobalData.RarityDegree, float>();
    private Dictionary<Type, Dictionary<object, ModifierValuesAndNames>> ModifiersDictionary = new Dictionary<Type, Dictionary<object, ModifierValuesAndNames>>();
    private Dictionary<object, int> SelectedModifierSelectionCountDict = new Dictionary<object, int>();
    public static LevelUpBuffController Instance;
    #endregion
    #region FlippinCardSetup

    private void AddEnumValuesToModifiersList(Type enumType)
    {

        Dictionary<object, ModifierValuesAndNames> enumValues = EnumValuesList(enumType);

        if (!ModifiersDictionary.ContainsKey(enumType))
        {
            ModifiersDictionary.Add(enumType, enumValues);
        }
        else
        {
            Debug.LogWarning($"{enumType.Name}.");
        }
        foreach (var item in Enum.GetValues(enumType))
        {
            SelectedModifierSelectionCountDict.Add(item, 0);
        }
 


    }
    private void AddRaritiesAndValues()
    {

        int numLevels = Enum.GetValues(typeof(GlobalData.RarityDegree)).Length;

        // Azalan deðerler için bir formül kullanýyoruz (örneðin, 1 / (2^i))
        float[] values = new float[numLevels];
        for (int i = 0; i < numLevels; i++)
        {
            values[i] = 1f / Mathf.Pow(2, i);
        }

        // Toplam deðerleri normalize ederek toplamý 1 yapýyoruz
        float total = 0f;
        foreach (var value in values)
        {
            total += value;
        }
        for (int i = 0; i < numLevels; i++)
        {
            values[i] /= total;
        }

        for (int i = 0; i < numLevels; i++)
        {
            BuffRarity_Value.Add((GlobalData.RarityDegree)i, values[i]);
        }



    }
    private void SetRaritiesAndValues()
    {
        float luckValue = GlobalModifiersCalculator.GetModifiersValue(typeof(GlobalData.PlayerModifiers), (int)GlobalData.PlayerModifiers.Luck);

        int numLevels = Enum.GetValues(typeof(GlobalData.RarityDegree)).Length;

        // Azalan deðerler için bir formül kullanýyoruz (örneðin, 1 / (2^i))
        float[] values = new float[numLevels];
        for (int i = 0; i < numLevels; i++)
        {
            values[i] = 1f / Mathf.Pow(2 - (luckValue - 1), i);
        }

        // Toplam deðerleri normalize ederek toplamý 1 yapýyoruz
        float total = 0f;
        foreach (var value in values)
        {
            total += value;
        }
        for (int i = 0; i < numLevels; i++)
        {
            values[i] /= total;
        }

        for (int i = 0; i < numLevels; i++)
        {
            BuffRarity_Value.Add((GlobalData.RarityDegree)i, values[i]);
        }


    }




    private ModifierValuesAndNames GetModifierWeights(Type enumType, object key)
    {
        switch (enumType)
        {
            case Type t when t == typeof(GlobalData.DamageModifiers):
                switch (key)
                {
                    case GlobalData.DamageModifiers allDamageTypes when allDamageTypes == GlobalData.DamageModifiers.allDamageTypes:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 1f * ModifierMultp,modifierWeightToCalculation = 0.01f * ModifierMultp, modifierObject = GlobalData.DamageModifiers.allDamageTypes, modifierNameToItemName = "All Damage" };

                    case GlobalData.DamageModifiers critical when critical == GlobalData.DamageModifiers.critical:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 2f * ModifierMultp, modifierWeightToCalculation = 0.02f* ModifierMultp, modifierObject = GlobalData.DamageModifiers.critical, modifierNameToItemName = "Critical Damage" };

                    default:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 1f, modifierWeightToCalculation = 0.2f, modifierObject = GlobalData.DamageModifiers.critical, modifierNameToItemName = "Standard" };
                }
            case Type t when t == typeof(GlobalData.MagicalDamageType):
                switch (key)
                {
                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Bleed:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 5f * ModifierMultp, modifierWeightToCalculation = 0.05f* ModifierMultp, modifierObject = GlobalData.MagicalDamageType.Bleed, modifierNameToItemName = "Bleed Damage" };

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Burn:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 5f * ModifierMultp, modifierWeightToCalculation = 0.05f * ModifierMultp, modifierObject = GlobalData.MagicalDamageType.Burn, modifierNameToItemName = "Burn Damage" };

                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Freeze:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 5f * ModifierMultp, modifierWeightToCalculation = 0.05f * ModifierMultp, modifierObject = GlobalData.MagicalDamageType.Freeze, modifierNameToItemName = "Frost Damage" };

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Poison:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 5f * ModifierMultp, modifierWeightToCalculation = 0.05f * ModifierMultp, modifierObject = GlobalData.MagicalDamageType.Poison, modifierNameToItemName = "Poison Damage" };

                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Shock:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 5f * ModifierMultp, modifierWeightToCalculation = 0.05f * ModifierMultp, modifierObject = GlobalData.MagicalDamageType.Shock, modifierNameToItemName = "Shock Damage" };

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Standard:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 5f * ModifierMultp, modifierWeightToCalculation = 0.05f * ModifierMultp, modifierObject = GlobalData.MagicalDamageType.Standard, modifierNameToItemName = "Standard Damage" };

                    default:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 5f * ModifierMultp, modifierWeightToCalculation = 0.05f * ModifierMultp, modifierObject = GlobalData.MagicalDamageType.Standard, modifierNameToItemName = "Standard" };

                }
            case Type t when t == typeof(GlobalData.ProjectileModifiers):
                switch (key)
                {


                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.BoltColliderScale:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 1f * ModifierMultp, modifierWeightToCalculation = 0.01f * ModifierMultp, modifierObject = GlobalData.ProjectileModifiers.BoltColliderScale, modifierNameToItemName = "Projectile Size" };

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.BoltDamage:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 2f * ModifierMultp, modifierWeightToCalculation = 0.02f * ModifierMultp, modifierObject = GlobalData.ProjectileModifiers.BoltDamage, modifierNameToItemName = "Projectile Damage" };

                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.BoltSpeed:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 2f * ModifierMultp, modifierWeightToCalculation = 0.02f * ModifierMultp, modifierObject = GlobalData.ProjectileModifiers.BoltSpeed, modifierNameToItemName = "Projectile Speed" };

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.CoolDown:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 1f * ModifierMultp, modifierWeightToCalculation = 0.01f * ModifierMultp, modifierObject = GlobalData.ProjectileModifiers.CoolDown, modifierNameToItemName = "Cooldown Reduction" };

                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.ExplosionDamage:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 3f * ModifierMultp, modifierWeightToCalculation = 0.03f * ModifierMultp, modifierObject = GlobalData.ProjectileModifiers.ExplosionDamage, modifierNameToItemName = "Explosion Damage" };

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.HowManyTimesCanPierceEnemy:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 1, modifierWeightToCalculation = 1, modifierObject = GlobalData.ProjectileModifiers.HowManyTimesCanPierceEnemy, removePercentage = true, modifierNameToItemName = "Pierce Count" };
                    //                                    { modifierWeight = Math.Clamp(1f * ModifierMultp, 1, 2), 
                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.MagicalPiercingPercentage:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 1f * ModifierMultp,modifierWeightToCalculation = 1, modifierObject= GlobalData.ProjectileModifiers.MagicalPiercingPercentage, modifierNameToItemName = "Magic Piercing", };

                    default:
                        return new ModifierValuesAndNames { modifierWeightToCardString = 0.1f, modifierWeightToCalculation = 0.1f, modifierObject = GlobalData.ProjectileModifiers.ExplosionDamage, modifierNameToItemName = "Standard" };
                }
        }
        return new ModifierValuesAndNames { modifierWeightToCardString = 0.1f, modifierWeightToCalculation = 0.1f, modifierNameToItemName = "Standard" }; ; ;


    }
    private Dictionary<object, ModifierValuesAndNames> EnumValuesList(Type enumType)
    {

        Dictionary<object, ModifierValuesAndNames> valueDictionary = new Dictionary<object, ModifierValuesAndNames>();

        foreach (var key in Enum.GetValues(enumType))
        {
            ModifierValuesAndNames modifierValuesAndNames = GetModifierWeights(enumType, key);
            valueDictionary.Add(key, modifierValuesAndNames);
        }
        return valueDictionary;
    }

    #region FlipCards

    public void SetFlippingCardObjects()
    {

        List<object> shuffledModifiers = new List<object>();
        foreach (Dictionary<object, ModifierValuesAndNames> Values in ModifiersDictionary.Values)
        {
            shuffledModifiers.AddRange(Values.Keys);
        }

        var rnd = UnityEngine.Random.Range(0, 1f);
        GlobalData.RarityDegree rarityDegree = GlobalData.RarityDegree.Common;
        foreach (var (rarityType, value) in BuffRarity_Value)
        {
            if (rnd <= value)
            {
                rarityDegree = rarityType;
                break;
            }
        }



        ShuffleListSet(shuffledModifiers);
        int whileIndex = 0;


        while (true)
        {
            if (whileIndex == 4) break;
            float rarityDegreeMultp = 1 + 0.3f * (((int)rarityDegree));
            var card = Instantiate(GetFlippingObject(shuffledModifiers[whileIndex].GetType(), shuffledModifiers[whileIndex]));

            card.transform.SetParent(_cardPositions[whileIndex].transform, false);
            var text = card.GetComponentInChildren<TMP_Text>();
            ModifierValuesAndNames modifierValuesAndNames = ModifiersDictionary[shuffledModifiers[whileIndex].GetType()][shuffledModifiers[whileIndex]];
            card.GetComponent<FlippingCard>().modifierValuesAndNames = modifierValuesAndNames;
            text.GetComponent<TMP_Text>().text = $"{modifierValuesAndNames.modifierNameToItemName}: +{Math.Ceiling(modifierValuesAndNames.modifierWeightToCardString * (1 + 0.2 * (int)rarityDegree))}" + (modifierValuesAndNames.removePercentage? "": "%");
            InstatiatedCards.Add(card);
            whileIndex++;
        }

        _backgroundImage.GetComponent<Image>().enabled = true;
        AudioController.PlayAudio_Delegate?.Invoke((int)AudioController.SoundTypes.levelup);
        Time.timeScale = 0f;

    }
    private GameObject GetFlippingObject(Type type, object key)
    {
        switch (type)
        {
            case Type t when t == typeof(GlobalData.DamageModifiers):
                switch (key)
                {
                    case GlobalData.DamageModifiers allDamageTypes when allDamageTypes == GlobalData.DamageModifiers.allDamageTypes:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.DamageModifiers critical when critical == GlobalData.DamageModifiers.critical:
                        return _flippingSword_CardCanvasPrefabs;

                    default:
                        return _flippingSword_CardCanvasPrefabs;
                }
            case Type t when t == typeof(GlobalData.MagicalDamageType):
                switch (key)
                {
                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Bleed:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Burn:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Freeze:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Poison:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Shock:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Standard:
                        return _flippingSword_CardCanvasPrefabs;

                    default:
                        return _flippingSword_CardCanvasPrefabs;
                }
            case Type t when t == typeof(GlobalData.ProjectileModifiers):
                switch (key)
                {



                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.BoltColliderScale:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.BoltDamage:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.BoltSpeed:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.CoolDown:
                        return _flippingBook_CardCanvasPrefabs;

                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.ExplosionDamage:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.HowManyTimesCanPierceEnemy:
                        return _flippingSword_CardCanvasPrefabs;

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.MagicalPiercingPercentage:
                        return _flippingSword_CardCanvasPrefabs;

                    default:
                        return _flippingSword_CardCanvasPrefabs;
                }

            case Type t when t == typeof(GlobalData.PlayerModifiers):
                switch (key)
                {
                    case GlobalData.PlayerModifiers.Speed:
                        return _flippingBook_CardCanvasPrefabs;

                    case GlobalData.PlayerModifiers.Hp:
                        return _flippingHearth_CardCanvasPrefabs;
                    case GlobalData.PlayerModifiers.Xp:
                        return _flippingBook_CardCanvasPrefabs;
                    case GlobalData.PlayerModifiers.CollectionRange:
                        return _flippingBook_CardCanvasPrefabs;
                    case GlobalData.PlayerModifiers.Resistance:
                        return _flippingArmor_CardCanvasPrefabs;
                    case GlobalData.PlayerModifiers.Luck:
                        return _flippingBook_CardCanvasPrefabs;
                    default:
                        return _flippingBook_CardCanvasPrefabs;
                }


        }
        return _flippingBook_CardCanvasPrefabs;
    }
    private void ShuffleListSet<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


    #endregion
    #endregion
    private void Start()
    {
        Instance = this;
        AddRaritiesAndValues();
        float luckValue = GlobalModifiersCalculator.GetModifiersValue(typeof(GlobalData.PlayerModifiers), (int)GlobalData.PlayerModifiers.Luck);
        AddEnumValuesToModifiersList(typeof(GlobalData.DamageModifiers));
        AddEnumValuesToModifiersList(typeof(GlobalData.MagicalDamageType));
        AddEnumValuesToModifiersList(typeof(GlobalData.ProjectileModifiers));

    }

    #region CardsOperations
    public void CardSelected(ModifierValuesAndNames modifierValuesAndNames)
    {
        if (SelectedModifierSelectionCountDict.ContainsKey(modifierValuesAndNames.modifierObject))
        {
            SelectedModifierSelectionCountDict[modifierValuesAndNames.modifierObject] += 1;
        }
        else
        {
            SelectedModifierSelectionCountDict.Add(modifierValuesAndNames.modifierObject, 1);
        }
        AudioController.PlayAudio_Delegate?.Invoke((int)AudioController.SoundTypes.powerup);
        GlobalModifiersCalculator.SetLevelUpModifiersDictionary(modifierValuesAndNames.modifierObject.ToString(), modifierValuesAndNames.modifierWeightToCalculation);
        _backgroundImage.GetComponent<Image>().enabled = false;
        foreach (var item in InstatiatedCards)
        {
            Destroy(item);
        }
        InstatiatedCards.Clear();
        Time.timeScale = 1f;
        CheckModifiersMaxAmouth();
    }
    #endregion

    private void CheckModifiersMaxAmouth()
    {
        if(SelectedModifierSelectionCountDict[GlobalData.ProjectileModifiers.CoolDown]>7)
        {
            ModifiersDictionary[typeof(GlobalData.ProjectileModifiers)].Remove(GlobalData.ProjectileModifiers.CoolDown);
        }
    }

}
public struct ModifierValuesAndNames
{
    public float modifierWeightToCardString;
    public float modifierWeightToCalculation;
    public object modifierObject;
    public string modifierNameToItemName;
    public bool removePercentage;
}