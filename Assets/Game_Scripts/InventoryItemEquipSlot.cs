using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemEquipSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject holdedObject;
    [SerializeField] private Item item;
    [SerializeField] private Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
        holdedObject = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (holdedObject == null)
        {
            PlacedObject(droppedObject);
        }

    }

    public void PlacedObject(GameObject droppedObject)
    {
        if (droppedObject.TryGetComponent<InventoryItemHolderButtonScript>(out InventoryItemHolderButtonScript inventoryItemHolderButtonScript))
        {
            if (inventoryItemHolderButtonScript.placedSlot != null)
            {
                if (inventoryItemHolderButtonScript.placedSlot.TryGetComponent<InventoryItemEquipSlot>(out InventoryItemEquipSlot inventoryItemEquipSlot))
                {
                    inventoryItemEquipSlot.ClearSlot();
                }
            }
        }
        ClearSlot();
        SetSlot(droppedObject, droppedObject.GetComponent<InventoryItemHolderButtonScript>().itemReferance);
        droppedObject.transform.SetParent(transform);
        droppedObject.transform.localPosition = Vector3.zero;
        droppedObject.GetComponent<InventoryItemHolderButtonScript>().PlaceItemInSlot(gameObject);
    }

    public void ClearSlot()
    {
        if (holdedObject != null)
        {
            Debug.LogError("ClearSlot called" + holdedObject.name + "  :gameObject: " + gameObject.name);
            image.raycastTarget = true;
            holdedObject.GetComponent<InventoryItemHolderButtonScript>().SlotTakenByOtherItems();
            InventoryManager.Instance.EditList(holdedObject, item, true);
            GlobalModifiersCalculator.SetItemModifiersDictionary(item, false);
            holdedObject = null;
        }
        else
        {
            Debug.LogError("holdedObject != null");
        }
    }
    public void SetSlot(GameObject gameObjct, Item item)
    {
        image.raycastTarget = false;
        if (holdedObject != null)
        {
            InventoryManager.Instance.EditList(holdedObject, item, false);
        }
        holdedObject = gameObjct;
        this.item = item;
        GlobalModifiersCalculator.SetItemModifiersDictionary(item, true);
    }
}
