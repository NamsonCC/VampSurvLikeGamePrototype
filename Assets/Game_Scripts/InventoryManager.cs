using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    [SerializeField] private GameObject copyToPrefabObject;
    [SerializeField] private List<Item> InventoryItemList = new List<Item>();
    [SerializeField] private List<GameObject> InverntoryButtonList = new List<GameObject>();
    [SerializeField] private GameObject InverntoryButtonsBase;
    [SerializeField] private GameObject EquipSlotBase;
    [SerializeField] private List<GameObject> EquipSlotObjects;
    [SerializeField] private List<GameObject> equipObjects;
    [SerializeField] private CanvasGroup myCanvasGroup;

    [SerializeField] private RectTransform content_RectTransform;
    private string inventorySavePath;
    private string inventoryIndexesSavePath;
    private string equipedSavePath;


    void Start()
    {
        Instance = this;
        myCanvasGroup = GetComponent<CanvasGroup>();
        if (Application.isEditor)
        {
            inventorySavePath = Path.Combine(Application.dataPath, "EditorSave/inventory.json");
            inventoryIndexesSavePath = Path.Combine(Application.dataPath, "EditorSave/inventoryIndexes.json");
            equipedSavePath = Path.Combine(Application.dataPath, "EditorSave/equipedItems.json");
        }
        else
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "SaveFiles/Inventories");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            inventorySavePath = Path.Combine(folderPath, "inventory.json");
            inventoryIndexesSavePath = Path.Combine(folderPath, "inventoryIndexes.json");
            equipedSavePath = Path.Combine(folderPath, "equipedItems.json");
        }

        if (Application.isEditor)
        {
            InventoryItemList = LootController.Instance.ChosenLootItemListForInventoryTest;
        }
        else
        {
            InventoryItemList = LoadInventory();
            equipObjects = LoadEquipedInventory();

            for (int i = 0; i < equipObjects.Count; i++)
            {
                EquipSlotObjects[i].GetComponent<InventoryItemEquipSlot>().PlacedObject(equipObjects[i]);
                equipObjects[i].GetComponent<InventoryItemHolderButtonScript>().EquipSavedItem(equipObjects[i]);
            }
        }
        SortItems("s", "s");
        CloseCanvas();
    }

    public void CloseCanvas()
    {
        myCanvasGroup.alpha = 0;
        myCanvasGroup.interactable = false;
        myCanvasGroup.blocksRaycasts = false;
        foreach (var item in InverntoryButtonList)
        {
            item.GetComponent<InventoryItemHolderButtonScript>().SetButton_UnClickable();
        }
    }

    public void OpenCanvas()
    {
        myCanvasGroup.alpha = 1f;
        myCanvasGroup.interactable = true;
        myCanvasGroup.blocksRaycasts = true;

        foreach (var item in InverntoryButtonList)
        {
            item.GetComponent<InventoryItemHolderButtonScript>().SetButton_Clickable();
        }
        SortItems("s", "s");

    }
    public void EditList(GameObject objct, Item item, bool add)
    {
        if (InverntoryButtonList.Contains(objct))
        {

            if (add == false)
            {
                InverntoryButtonList.Remove(objct);
                InventoryItemList.Remove(item);
            }
        }
        else
        {
            if (add == true)
            {
                InverntoryButtonList.Add(objct);
                InventoryItemList.Add(item);
            }
        }

        SortItemsByTextActiveStatus();
        Debug.LogError("   SortItems(;");
        SortItems("s", "s");
    }
    public void SortItemsByTextActiveStatus()
    {
        var gridLayoutGroup = content_RectTransform.GetComponent<GridLayoutGroup>();
        Transform[] children = new Transform[gridLayoutGroup.transform.childCount];
        for (int i = 0; i < gridLayoutGroup.transform.childCount; i++)
        {
            children[i] = gridLayoutGroup.transform.GetChild(i);
        }
        var sortedChildren = children.OrderBy(child =>
        {
            TMPro.TMP_Text textComponent = child.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (textComponent != null)
            {
                Debug.LogError("textComponent" + textComponent);
                return textComponent.gameObject.activeInHierarchy ? 1 : 0;
            }
            return 2;
        }).ToArray();


        for (int i = 0; i < sortedChildren.Length; i++)
        {
            sortedChildren[i].SetSiblingIndex(i);
        }

    }

    public void InventoryPanelClosed()
    {
        SaveInventory(InventoryItemList);
    }
    private void OnDisable()
    {
        SaveInventory(InventoryItemList);
    }
    private void OnApplicationQuit()
    {
        SaveInventory(InventoryItemList);
    }
    public void SaveInventory(List<Item> inventory)
    {
        Debug.Log("Inventory Saved");
        string json = JsonConvert.SerializeObject(inventory, Formatting.Indented);
        File.WriteAllText(inventorySavePath, json);
        HashSet<int> indexes = new HashSet<int>();
        foreach (var index in inventory)
        {
            indexes.Add(index.ItemIndex);
        }
        string jsonIndex = JsonConvert.SerializeObject(indexes, Formatting.Indented);
        File.WriteAllText(inventoryIndexesSavePath, jsonIndex);

        json = JsonConvert.SerializeObject(equipObjects, Formatting.Indented);
        File.WriteAllText(inventorySavePath, json);

    }

    public List<Item> LoadInventory()
    {
        if (File.Exists(inventorySavePath))
        {
            string json = File.ReadAllText(inventorySavePath);
            return JsonConvert.DeserializeObject<List<Item>>(json);
        }
        return new List<Item>();  // Eðer dosya yoksa boþ envanter döndür
    }
    public List<GameObject> LoadEquipedInventory()
    {
        if (File.Exists(equipedSavePath))
        {
            string json = File.ReadAllText(equipedSavePath);
            return JsonConvert.DeserializeObject<List<GameObject>>(json);
        }
        return new List<GameObject>();  // Eðer dosya yoksa boþ envanter döndür
    }

    public void SortItems(string modifierName, string rarityName)
    {
        var modifierObject = ParseModifier(modifierName);
        var rarityObject = ParseRarity(rarityName);

        List<Item> sortedList = new List<Item>();
        foreach (var item in InventoryItemList)
        {
            bool check1 = false;
            bool check2 = false;
            foreach (var key in item.Modifiers.Keys)
            {
                if (key.Equals(modifierObject) || modifierObject == null)
                {
                    check1 = true;
                    break;
                }
            }
            if (item.suffixName.Equals(rarityObject) || rarityObject == null)
            {
                check2 = true;
            }
            if (check1 && check2)
            {
                sortedList.Add(item);
            }
        }
        foreach (var item in InverntoryButtonList)
        {
            item.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        for (int i = 0; i < sortedList.Count; i++)
        {
            if (InverntoryButtonList[i] == null)
            {
                InverntoryButtonList.Add(Instantiate(copyToPrefabObject, InverntoryButtonsBase.transform));
            }
            InverntoryButtonList[i].GetComponent<InventoryItemHolderButtonScript>().SetButtonDataFromItem(sortedList[i]);
            InverntoryButtonList[i].GetComponent<CanvasGroup>().blocksRaycasts = true;
            InverntoryButtonList[i].GetComponent<CanvasGroup>().interactable = true;
        }
    }

    private object ParseModifier(string modifierName)
    {
        if (Enum.TryParse<GlobalData.DamageModifiers>(modifierName, out var damageModf)) return damageModf;
        if (Enum.TryParse<GlobalData.MagicalDamageType>(modifierName, out var magicalDamageType)) return magicalDamageType;
        if (Enum.TryParse<GlobalData.ProjectileModifiers>(modifierName, out var projectileModifiers)) return projectileModifiers;
        return null; // Eðer hiçbiri eþleþmezse null döndür
    }

    private object ParseRarity(string rarityName)
    {
        if (Enum.TryParse<GlobalData.RarityDegree>(rarityName, out var rarityDegree)) return rarityDegree;
        return null; // Eðer eþleþmezse null döndür
    }
    public void CollectItem(int availableLootableItemIndex)
    {
        InventoryItemList.Add(LootController.Instance.GetItem(availableLootableItemIndex));
        string std = "Inventory ItemList -:::-";
        foreach (var item in InventoryItemList)
        {
            std += item.ItemName + "  :::-";
        }
        Debug.Log(std);
    }

}