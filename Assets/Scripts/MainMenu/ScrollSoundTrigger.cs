using UnityEngine;
using UnityEngine.EventSystems; // 🔁 ¡IMPORTANTE!

public class ScrollSoundTrigger : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlayScroll();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlayClick();
    }
}
