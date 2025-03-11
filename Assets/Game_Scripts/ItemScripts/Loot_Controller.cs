using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class LootController : MonoBehaviour
{


    public static LootController Instance;
    public List<RarityandSprites> spritesByTypeAndRarity = new List<RarityandSprites>();

    public GameObject[] xpEntities;
    private Dictionary<Type, Dictionary<object, ModifierValuesAndNames>> ModifiersList = new Dictionary<Type, Dictionary<object, ModifierValuesAndNames>>();
    public int MaxLootAmouthEachTime;
    public List<Item> itemList = new List<Item>();
    public List<Item> ChosenLootItemList = new List<Item>();


    public List<Item> ChosenLootItemListForInventoryTest = new List<Item>();
    public List<Item> ChosenLootItemListForInventory = new List<Item>();


    public bool finished;
    int[] rareItemCounts = { 3000, 1500, 1000, 500, 250, 125, 60 };

    public enum LootTypes
    {
        Item,
        XP,
        Gold,
    }
    public enum ItemSuffixes
    {
        Amulet,
        Ring,
        Cloak,
        Orb,
        Talisman,
        Tome,
    }


    private void Start()
    {
        Instance = this;
        AddEnumValuesToModifiersList(typeof(GlobalData.DamageModifiers));
        AddEnumValuesToModifiersList(typeof(GlobalData.MagicalDamageType));
        AddEnumValuesToModifiersList(typeof(GlobalData.ProjectileModifiers));

        TextAsset jsonFile = Resources.Load<TextAsset>("Object_EntitiesDatas/JsonDatas/ItemList");
        TextAsset jsonFileInventoryIndexes = Resources.Load<TextAsset>("Object_EntitiesDatas/JsonDatas/inventoryIndexes");

        if (jsonFile != null)
        {

            using (StringReader stringReader = new StringReader(jsonFile.text))
            using (JsonTextReader reader = new JsonTextReader(stringReader))
            {

                JArray jsonArray = JArray.Load(reader);
                int totalItemCount = jsonArray.Count;
                if (totalItemCount == 0)
                {
                    Debug.LogError("JSON empty!");
                    return;
                }

                if (Application.isEditor)
                {
                    HashSet<int> chosenIndexesForInventoryTest = new HashSet<int>();

                    // Rastgele indeks seçme (tekrarsýz)
                    while (chosenIndexesForInventoryTest.Count < MaxLootAmouthEachTime && chosenIndexesForInventoryTest.Count < totalItemCount)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, totalItemCount);
                        chosenIndexesForInventoryTest.Add(randomIndex);
                    }

                    // Seçilen indekslere göre tek tek deserialize et
                    foreach (int index in chosenIndexesForInventoryTest)
                    {
                        JObject itemJson = (JObject)jsonArray[index];
                        Item item = itemJson.ToObject<Item>();
                        ChosenLootItemListForInventoryTest.Add(item);
                    }
                }

                using (StringReader StringReaderInventory = new StringReader(jsonFileInventoryIndexes.text))
                using (JsonTextReader ReaderInventory = new JsonTextReader(StringReaderInventory))
                {

                    JArray jsonArrayInventory = JArray.Load(ReaderInventory);
                    int totalItemCountInventory = jsonArrayInventory.Count;

                    if (totalItemCountInventory == 0)
                    {
                        Debug.LogError("JSON empty!");
                        return;
                    }


                    HashSet<int> chosenIndexesForLoot = new HashSet<int>();
                    for (int index = 0; index < totalItemCountInventory; index++)
                    {
                        chosenIndexesForLoot.Add(jsonArrayInventory[index].Value<int>());
                    }

                    // Rastgele indeks seçme (tekrarsýz)
                    while (chosenIndexesForLoot.Count < MaxLootAmouthEachTime + totalItemCountInventory && chosenIndexesForLoot.Count < totalItemCount)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, totalItemCount);
                        chosenIndexesForLoot.Add(randomIndex);
                    }
                    for (int index = 0; index < totalItemCountInventory; index++)
                    {
                        chosenIndexesForLoot.Remove(jsonArrayInventory[index].Value<int>());
                    }

                    // Seçilen indekslere göre tek tek deserialize et
                    foreach (int index in chosenIndexesForLoot)
                    {
                        JObject itemJson = (JObject)jsonArray[index];
                        Item item = itemJson.ToObject<Item>();
                        ChosenLootItemListForInventory.Add(item);
                    }
                }
            }


            finished = true;
        }
        else
        {
            CreateItems();
        }

    }
    #region ItemSetup
    #region GetModifiersWeights
    private struct ModifierValuesAndNames
    {
        public float modifierWeight;
        public string modifierNameToItemName;
    }
    private void AddEnumValuesToModifiersList(Type enumType)
    {

        Dictionary<object, ModifierValuesAndNames> enumValues = EnumValuesList(enumType);

        if (!ModifiersList.ContainsKey(enumType))
        {
            ModifiersList.Add(enumType, enumValues);
        }
        else
        {
            Debug.LogWarning($"{enumType.Name}.");
        }
    }
    private Dictionary<object, ModifierValuesAndNames> EnumValuesList(Type enumType)
    {
        Dictionary<object, ModifierValuesAndNames> valueDictionary = new Dictionary<object, ModifierValuesAndNames>();
        foreach (var key in Enum.GetValues(enumType))
        {
            valueDictionary.Add(key, GetModifierWeights(enumType, key));
        }
        return valueDictionary;
    }

    private ModifierValuesAndNames GetModifierWeights(Type enumType, object key)
    {
        switch (enumType)
        {
            case Type t when t == typeof(GlobalData.DamageModifiers):
                switch (key)
                {
                    case GlobalData.DamageModifiers allDamageTypes when allDamageTypes == GlobalData.DamageModifiers.allDamageTypes:
                        return new ModifierValuesAndNames { modifierWeight = 0.2f, modifierNameToItemName = "Omnipotent" };

                    case GlobalData.DamageModifiers critical when critical == GlobalData.DamageModifiers.critical:
                        return new ModifierValuesAndNames { modifierWeight = 0.1f, modifierNameToItemName = "Critical Strike" };

                    default:
                        return new ModifierValuesAndNames { modifierWeight = 1f, modifierNameToItemName = "Standard" };
                }
            case Type t when t == typeof(GlobalData.MagicalDamageType):
                switch (key)
                {
                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Bleed:
                        return new ModifierValuesAndNames { modifierWeight = 0.75f, modifierNameToItemName = "Bleeding" };

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Burn:
                        return new ModifierValuesAndNames { modifierWeight = 0.75f, modifierNameToItemName = "Flaming" };

                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Freeze:
                        return new ModifierValuesAndNames { modifierWeight = 0.75f, modifierNameToItemName = "Frozen" };

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Poison:
                        return new ModifierValuesAndNames { modifierWeight = 0.75f, modifierNameToItemName = "Venomous" };

                    case GlobalData.MagicalDamageType allDamageTypes when allDamageTypes == GlobalData.MagicalDamageType.Shock:
                        return new ModifierValuesAndNames { modifierWeight = 0.75f, modifierNameToItemName = "Shocking" };

                    case GlobalData.MagicalDamageType critical when critical == GlobalData.MagicalDamageType.Standard:
                        return new ModifierValuesAndNames { modifierWeight = 0.75f, modifierNameToItemName = "Standard" };

                    default:
                        return new ModifierValuesAndNames { modifierWeight = 0.75f, modifierNameToItemName = "Standard" };
                }
            case Type t when t == typeof(GlobalData.ProjectileModifiers):
                switch (key)
                {



                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.BoltColliderScale:
                        return new ModifierValuesAndNames { modifierWeight = 0.2f, modifierNameToItemName = "Scaler" };

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.BoltDamage:
                        return new ModifierValuesAndNames { modifierWeight = 0.3f, modifierNameToItemName = "Powerful" };

                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.BoltSpeed:
                        return new ModifierValuesAndNames { modifierWeight = 0.1f, modifierNameToItemName = "Swift" };

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.CoolDown:
                        return new ModifierValuesAndNames { modifierWeight = 0.1f, modifierNameToItemName = "Rapid" };

                    case GlobalData.ProjectileModifiers allDamageTypes when allDamageTypes == GlobalData.ProjectileModifiers.ExplosionDamage:
                        return new ModifierValuesAndNames { modifierWeight = 0.3f, modifierNameToItemName = "Explosive" };

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.HowManyTimesCanPierceEnemy:
                        return new ModifierValuesAndNames { modifierWeight = 0.01f, modifierNameToItemName = "Multi Piercing" };

                    case GlobalData.ProjectileModifiers critical when critical == GlobalData.ProjectileModifiers.MagicalPiercingPercentage:
                        return new ModifierValuesAndNames { modifierWeight = 0.1f, modifierNameToItemName = "Penetrating" };

                    default:
                        return new ModifierValuesAndNames { modifierWeight = 0.1f, modifierNameToItemName = "Standard" }; ;
                }
        }
        return new ModifierValuesAndNames { modifierWeight = 0.1f, modifierNameToItemName = "Standard" }; ; ;
    }
    #endregion

    #region CreateItem
    private void CreateItems()
    {
        string logString = "";
        try
        {
            List<object> shuffledModifiers = new List<object>();
            foreach (Dictionary<object, ModifierValuesAndNames> Values in ModifiersList.Values)
            {
                shuffledModifiers.AddRange(Values.Keys);
            }
            for (int RarirtyIndex = 0; RarirtyIndex < Enum.GetValues(typeof(GlobalData.RarityDegree)).Length; RarirtyIndex++)
            {
                GlobalData.RarityDegree rarityDegree = (GlobalData.RarityDegree)RarirtyIndex;
                for (int i = 0; i < rareItemCounts[RarirtyIndex]; i++)
                {

                    ShuffleListSet(shuffledModifiers);
                    int whileIndex = 0;
                    int howManyModifierCanBeAdded = GetNumberOfModifierByRarity(rarityDegree);
                    Item item = new Item();
                    item.ItemIndex = itemList.Count;
                    int whichTypeOfItem = UnityEngine.Random.Range(0, Enum.GetValues(typeof(ItemSuffixes)).Length);
                    itemList.Add(item);
                    string nameOfItem = $"{rarityDegree.ToString()} ";
                    while (true)
                    {
                        if (whileIndex == howManyModifierCanBeAdded) break;
                        logString += $" {itemList[i]}";
                        logString += $" {shuffledModifiers[whileIndex]}";
                        logString += $" {ModifiersList[shuffledModifiers[whileIndex].GetType()]}";
                        logString += $" {ModifiersList[shuffledModifiers[whileIndex].GetType()][shuffledModifiers[whileIndex]]}";

                        float rarityDegreeMultp = 1 + 0.3f * (((int)rarityDegree));
                        item.Modifiers.Add(shuffledModifiers[whileIndex], Math.Ceiling((ModifiersList[shuffledModifiers[whileIndex].GetType()][shuffledModifiers[whileIndex]].modifierWeight) * (10 - howManyModifierCanBeAdded) * rarityDegreeMultp));
                        nameOfItem += $"{ModifiersList[shuffledModifiers[whileIndex].GetType()][shuffledModifiers[whileIndex]].modifierNameToItemName} ";
                        whileIndex++;
                    }
                    item.rarityName = rarityDegree;
                    item.suffixName = (ItemSuffixes)whichTypeOfItem;
                    item.ItemName = nameOfItem + $"{item.suffixName.ToString()}";
                    item.SetSerializedModifiers();
                }
            }
            SaveItemListToTextFile();
            Debug.Log(logString);
        }
        catch (Exception e)
        {
            Debug.LogError(logString);
            throw e;
        }


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
    private int GetNumberOfModifierByRarity(GlobalData.RarityDegree rarityDegree)
    {
        int modf = UnityEngine.Random.Range(1, 5);

        int integer = Math.Clamp(((int)rarityDegree) + modf, 1, 6);
        return UnityEngine.Random.Range(1, integer); ;
    }
    private void SaveItemListToTextFile()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePathJson = desktopPath + "\\a\\ItemList.json";
        string json = JsonConvert.SerializeObject(itemList, Formatting.Indented);
        File.WriteAllText(filePathJson, json);
    }
    #endregion
    #endregion




    public (int suffixName, int rarityName, int) GetLoot()
    {
        int rand = UnityEngine.Random.Range(0, ChosenLootItemListForInventory.Count);
        return ((int)ChosenLootItemListForInventory[rand].suffixName, (int)ChosenLootItemListForInventory[rand].rarityName, rand);
    }
    public Item GetItem(int availableItemIndex)
    {
        return ChosenLootItemListForInventory[availableItemIndex];
    }
}

[Serializable]
public class RarityandSprites
{
    public List<Sprite> rarityToSprite = new List<Sprite>();
}

[Serializable]
public class Item
{
    public int ItemIndex;
    public string ItemName;
    public GlobalData.RarityDegree rarityName;
    public LootController.ItemSuffixes suffixName;
    public Dictionary<object, double> Modifiers = new Dictionary<object, double>();
    public string modifiersAndValues;

    public void SetSerializedModifiers()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        foreach (var modifier in Modifiers)
        {
            sb.Append($"\"{modifier.Key}\": {modifier.Value},");
        }
        if (Modifiers.Count > 0)
        {
            sb.Length--; // Son virgülü kaldýr
        }
        sb.Append("}");
        modifiersAndValues = sb.ToString();
    }

    private (Type, int) GetModifiersType(string nameOfModf)
    {
        if (Enum.TryParse<GlobalData.DamageModifiers>(nameOfModf, out var damageModf))
        {
            return (typeof(GlobalData.DamageModifiers), (int)damageModf);
        }
        else if (Enum.TryParse<GlobalData.MagicalDamageType>(nameOfModf, out var magicalDamageType))
        {
            return (typeof(GlobalData.MagicalDamageType), (int)magicalDamageType);
        }
        else if (Enum.TryParse<GlobalData.ProjectileModifiers>(nameOfModf, out var projectileModifiers))
        {
            return (typeof(GlobalData.ProjectileModifiers), (int)projectileModifiers);
        }
        throw new Exception("type error");
    }

    public void SetModifiers()
    {
        Dictionary<string, float> modifiers = JsonConvert.DeserializeObject<Dictionary<string, float>>(modifiersAndValues);
        foreach (var item in modifiers)
        {
            (Type type, int enumIndex) = GetModifiersType(item.Key);

            Enum enumValue = (Enum)Enum.ToObject(type, enumIndex);
            Modifiers.Add(enumValue, item.Value);
            Debug.LogError($"{type} {item.Value}");
        }

    }
}





