using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;


public class FlippingCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler,IDeselectHandler
{
    public ModifierValuesAndNames modifierValuesAndNames;
    [SerializeField] private ParticleSystem _particleSystem;
    private void Start()
    {
        if (TryGetComponent(out Animator animator))
        {
            animator.enabled = false;
            _particleSystem.Stop();
        }
    }
    #region IpointerInterfacesAreas
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.1f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        _particleSystem.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1.0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        _particleSystem.Stop();
    }

    public void OnSelect(BaseEventData eventData)
    {
        LevelUpBuffController.Instance.CardSelected(modifierValuesAndNames);
        _particleSystem.Play();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _particleSystem.Stop();
    }


    #endregion
}
