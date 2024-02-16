using UnityEngine;
using UnityEngine.EventSystems;

public class UIInteractionSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        UISoundManager.Instance.PlaySound(UISoundDefinitions.UIEvent.MouseEnter);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UISoundManager.Instance.PlaySound(UISoundDefinitions.UIEvent.MousePress);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UISoundManager.Instance.PlaySound(UISoundDefinitions.UIEvent.MouseRelease);
    }
}
