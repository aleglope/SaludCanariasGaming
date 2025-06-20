using UnityEngine;

public class UIPanelPopUpAnimator : MonoBehaviour
{
    public float popUpDuration = 0.5f;
    public Vector3 hiddenScale = Vector3.zero;
    public Vector3 shownScale = Vector3.one;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        LeanTween.scale(gameObject, shownScale, popUpDuration).setEaseOutBack();
    }

    public void HidePanel()
    {
        LeanTween.scale(gameObject, hiddenScale, popUpDuration)
            .setEaseInBack()
            .setOnComplete(() => gameObject.SetActive(false));
    }

    public void HideInstantly()
    {
        gameObject.SetActive(false);
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.localScale = hiddenScale;
    }
}
