using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemHolderButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject TextObject;
    [SerializeField] private GameObject ItemSpriteObject;
    [SerializeField] private int SpriteTextureIndex;

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform startParent;
    [SerializeField] private Vector2 startPosition;

    [SerializeField] private bool placedToSlot;
    public GameObject placedSlot;

    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [SerializeField] public Item itemReferance;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.anchoredPosition;
        startParent = rectTransform.parent; // Baþlangýç parent'ýný kaydet
        gridLayoutGroup = GetComponentInParent<GridLayoutGroup>();
    }

    private void Start()
    {

    }

    public void SetButtonDataFromItem(Item item)
    {
        TextObject.SetActive(false);
        ItemSpriteObject.GetComponent<Image>().sprite = LootController.Instance.spritesByTypeAndRarity[(int)item.suffixName].rarityToSprite[(int)item.rarityName];
        ItemSpriteObject.SetActive(true);
        itemReferance = item;
    }

    public void SetButton_Clickable()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void SetButton_UnClickable()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }


    public void OnSelect(BaseEventData eventData)
    {

    }
    public void OnDeselect(BaseEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData) => ScaleButton(1.25f);
    public void OnPointerExit(PointerEventData eventData) => ScaleButton(1.0f);

    private void ScaleButton(float scale)
    {
        transform.DOScale(scale, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        // Sürüklenen objeyi Canvas'a taþý (Scroll View dýþýna çýkar)
        rectTransform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // UI elementini fare ile sürükle
        if (gridLayoutGroup != null) gridLayoutGroup.enabled = false;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.enabled = true;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        if (gridLayoutGroup != null) gridLayoutGroup.enabled = true;

        HandleDrop(eventData);
    }
    private void HandleDrop(PointerEventData eventData)
    {
        InventoryItemHolderButtonScript inventoryItemHolderButtonScript;
        if (eventData.pointerEnter != null)
        {

            if (eventData.pointerEnter.TryGetComponent<InventoryItemHolderButtonScript>(out inventoryItemHolderButtonScript)
                || eventData.pointerEnter.gameObject.transform.parent.TryGetComponent<InventoryItemHolderButtonScript>(out inventoryItemHolderButtonScript))
            {
                Debug.LogError("I was hello there 0");
                if (inventoryItemHolderButtonScript.placedToSlot == true)
                {
                    if (inventoryItemHolderButtonScript.placedSlot != null)
                    {
                        if (placedSlot != null)
                        {
                            placedSlot.GetComponent<InventoryItemEquipSlot>().ClearSlot();
                        }
                        inventoryItemHolderButtonScript.placedSlot.GetComponent<InventoryItemEquipSlot>().PlacedObject(gameObject);
                        Debug.LogError("I was hello there 1");
                    }
                    Debug.LogError("I was hello there 2");
                    return;
                }
            }

            if (!eventData.pointerEnter.GetComponent<InventoryItemEquipSlot>())
            {

                if (placedSlot != null)
                {
                    placedSlot.GetComponent<InventoryItemEquipSlot>().ClearSlot();
                }
                else
                {
                    SetToStart();
                    InventoryManager.Instance.SortItemsByTextActiveStatus();
                }
                Debug.LogError("I was not dropped : " + eventData.pointerEnter.name);
            }
        }
        else
        {
            if (placedSlot != null)
            {
                placedSlot.GetComponent<InventoryItemEquipSlot>().ClearSlot();
            }
            else
            {
                SetToStart();
                InventoryManager.Instance.SortItemsByTextActiveStatus();
            }

        }
    }




    public void PlaceItemInSlot(GameObject gameObjct)
    {
        placedToSlot = true;
        placedSlot = gameObjct;
    }


    private void SetToStart()
    {
        placedSlot = null;
        placedToSlot = false;
        rectTransform.SetParent(startParent);
        rectTransform.anchoredPosition = startPosition;
        // InventoryManager.Instance.EditList(gameObject, itemReferance, true);
    }

    //This object's slot taken
    public void SlotTakenByOtherItems()
    {
        SetToStart();
        InventoryManager.Instance.EditList(gameObject, itemReferance, true);
    }

    public void EquipSavedItem(GameObject gameObjct)
    {
        placedToSlot = true;
        placedSlot = gameObjct;
    }
}
