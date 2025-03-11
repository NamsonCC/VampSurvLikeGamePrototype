using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        try
        {
            transform.DOScale(1.2f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        }
        catch (System.Exception e)
        {
            Debug.Log("System.Exception dotween" + e);
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        try
        {
            transform.DOScale(1.0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        }
        catch (System.Exception e)
        {
            Debug.Log("System.Exception dotween" + e);
        }

    }
}
