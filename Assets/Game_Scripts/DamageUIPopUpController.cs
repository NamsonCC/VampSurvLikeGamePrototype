using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class DamageUIPopUpController : MonoBehaviour
{
    public static DamageUIPopUpController Instance;

    [SerializeField] private GameObject damageGuiPrefab;
    [SerializeField] private GameObject BaseCanvas;
    private Queue<TMP_TextData> damagePopUpGUIQueue = new Queue<TMP_TextData>();
    public List<TMP_TextData> Activated_damagePopUpGUIList = new List<TMP_TextData>();


    private void Start()
    {
        Instance = this;
        var textObjects = BaseCanvas.GetComponentsInChildren<TMP_Text>(true);
        foreach (var text in textObjects)
        {
            text.enabled = true;
            text.text = "";
            TMP_TextData tMP_TextData = new TMP_TextData(text.gameObject);
            damagePopUpGUIQueue.Enqueue(tMP_TextData);
        }

    }
    public void DisableDamageGui(List<TMP_TextData> tMP_TextList)
    {
        foreach (var tmpText in tMP_TextList)
        {
            tmpText.ResetData();
            Activated_damagePopUpGUIList.Remove(tmpText);
            damagePopUpGUIQueue.Enqueue(tmpText);
        }

    }
    public void EnableDamageGui(int damage, float3 targetPosition)
    {

        if (damagePopUpGUIQueue.Count > 0)
        {
            TMP_TextData damageUI = damagePopUpGUIQueue.Dequeue();

            damageUI.tMP_Text.gameObject.SetActive(true);
            if (damage == 0)
            {
                Debug.LogError("damage == 0");
            }
            damageUI.tMP_Text.text = damage.ToString();
            damageUI.tMP_Text.gameObject.transform.position = targetPosition;
            Activated_damagePopUpGUIList.Add(damageUI);

        }
        else
        {
            GameObject damageUIObject = Instantiate(damageGuiPrefab, BaseCanvas.transform);
            TMP_TextData tMP_TextData = new TMP_TextData(damageUIObject);
            Activated_damagePopUpGUIList.Add(tMP_TextData);
        }
    }

}

public class TMP_TextData
{
    public TMP_Text tMP_Text;
    public float moveSpeed;
    public bool isFadingOut;

    public TMP_TextData(GameObject tMP_TextObject)
    {
        tMP_Text = tMP_TextObject.GetComponent<TMP_Text>();
        tMP_Text.alpha = 0;
        moveSpeed = 10f;
        isFadingOut = false;
    }


    public void ResetData()
    {
        moveSpeed = 10f;
        tMP_Text.alpha = 0;
        isFadingOut = false;
        tMP_Text.gameObject.SetActive(false);
    }
}


public partial class DamageUIManagedSystemController : SystemBase
{
    private EntityManager _entityManager;
    private EntityQuery _totalDamageToUI;

    float updateInterval = 1f / 30f;
    float passedTime = 0f;
    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _totalDamageToUI = _entityManager.CreateEntityQuery(typeof(TotalDamageToMonsters));
    }

    protected override void OnUpdate()
    {
        if (updateInterval > passedTime)
        {
            passedTime += SystemAPI.Time.DeltaTime;
            return;
        }
        passedTime = 0f;
        passedTime += SystemAPI.Time.DeltaTime;


        var TotalDamageToMonstersQuerry = _totalDamageToUI.ToEntityArray(Unity.Collections.Allocator.Temp);
        foreach (var totalDamage in TotalDamageToMonstersQuerry)
        {
            TotalDamageToMonsters totalDamageToMonsters = _entityManager.GetComponentData<TotalDamageToMonsters>(totalDamage);
            DamageUIPopUpController.Instance.EnableDamageGui((int)totalDamageToMonsters.totalDamage, totalDamageToMonsters.location);
            AudioController.PlayAudio_Delegate?.Invoke((int)AudioController.SoundTypes.hit);
        }


        _entityManager.DestroyEntity(TotalDamageToMonstersQuerry);
        TotalDamageToMonstersQuerry.Dispose();

        float deltaTime = SystemAPI.Time.DeltaTime;
        List<TMP_TextData> list = new List<TMP_TextData>();



        foreach (var damagePopUp in DamageUIPopUpController.Instance.Activated_damagePopUpGUIList)
        {
            if (damagePopUp.isFadingOut == false)
            {
                damagePopUp.tMP_Text.alpha += 1000f * deltaTime;
                if (damagePopUp.tMP_Text.alpha >= 255f)
                {
                    damagePopUp.tMP_Text.alpha = 255f;
                    damagePopUp.isFadingOut = true;
                }
            }
            else
            {
                damagePopUp.tMP_Text.alpha -= 1000f * deltaTime;
                if (damagePopUp.tMP_Text.alpha <= 0)
                {
                    damagePopUp.tMP_Text.alpha = 0;
                    list.Add(damagePopUp);
                }
            }
            damagePopUp.tMP_Text.rectTransform.anchoredPosition += Vector2.up * damagePopUp.moveSpeed * deltaTime;
            damagePopUp.moveSpeed = Mathf.Max(1f, damagePopUp.moveSpeed - 10f * deltaTime * 2);

        }
        DamageUIPopUpController.Instance.DisableDamageGui(list);
    }
}