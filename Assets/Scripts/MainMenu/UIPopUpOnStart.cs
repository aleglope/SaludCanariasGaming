using UnityEngine;

public class UIPopUpOnStart : MonoBehaviour
{
    public float duration = 0.5f;
    public Vector3 hiddenScale = new Vector3(0f, 0f, 0f);
    public Vector3 shownScale = Vector3.one;

    void Start()
    {
        transform.localScale = hiddenScale;
        LeanTween.scale(gameObject, shownScale, duration).setEaseOutBack();
    }
}
