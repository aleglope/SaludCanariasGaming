using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIImageAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image fondo; // Asigna manualmente el fondo del panel (no los hijos)

    private float originalAlpha = 1f;
    private float fadedAlpha = 0.6f;
    private float duration = 0.2f;

    private void Start()
    {
        if (fondo == null)
            fondo = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.value(fondo.gameObject, SetAlpha, originalAlpha, fadedAlpha, duration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.value(fondo.gameObject, SetAlpha, fondo.color.a, originalAlpha, duration);
    }

    private void SetAlpha(float alpha)
    {
        Color c = fondo.color;
        c.a = alpha;
        fondo.color = c;
    }
}
