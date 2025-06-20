using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlatoUIManager : MonoBehaviour
{
    [Header("Panel Izquierdo")]
    public GameObject panelIzquierdo;
    public Image imageLeft;
    public TextMeshProUGUI titleLeft;
    public TextMeshProUGUI caloriesLeft;
    public TextMeshProUGUI beneficioLeft;

    [Header("Panel Derecho")]
    public GameObject panelDerecho;
    public Image imageRight;
    public TextMeshProUGUI titleRight;
    public TextMeshProUGUI caloriesRight;
    public TextMeshProUGUI beneficioRight;

    [Header("Panel Central")]
    public GameObject panelCentral;
    public Image imageCentral;

    private static readonly Color colorCorrecto = new Color32(0x04, 0x68, 0xA9, 255);     // #0468a9
    private static readonly Color colorIncorrecto = new Color32(0xD8, 0x8F, 0x1F, 255);  // #d88f1f
    private static readonly Color colorNeutral = new Color32(39, 39, 39, 255);          // sin cambios

    private void Start()
    {
        // Escala inicial segura
        imageLeft.rectTransform.localScale = Vector3.one;
        imageRight.rectTransform.localScale = Vector3.one;
    }

    public void SetPlatoLeft(PlatoData plato, bool mostrarCalorias = false)
    {
        titleLeft.text = plato.nombre;
        imageLeft.sprite = plato.imagen;
        caloriesLeft.text = mostrarCalorias ? $"{plato.caloriasPorRacion} kcal" : "???";
        beneficioLeft.text = mostrarCalorias ? plato.beneficio : "";

        if (mostrarCalorias)
        {
            Pop(caloriesLeft.gameObject);
            Pop(beneficioLeft.gameObject);
        }

        ResetVisuals();
    }

    public void SetPlatoRight(PlatoData plato, bool mostrarCalorias = false)
    {
        titleRight.text = plato.nombre;
        imageRight.sprite = plato.imagen;
        caloriesRight.text = mostrarCalorias ? $"{plato.caloriasPorRacion} kcal" : "???";
        beneficioRight.text = mostrarCalorias ? plato.beneficio : "";

        if (mostrarCalorias)
        {
            Pop(caloriesRight.gameObject);
            Pop(beneficioRight.gameObject);
        }

        ResetVisuals();
    }

    public void SetPlatoCorrect(bool eligioDerecha)
    {
        ColorTransition(colorCorrecto);

        if (eligioDerecha)
            LeanTween.scale(imageRight.rectTransform, Vector3.one * 0.95f, 0.3f).setEaseOutBack();
        else
            LeanTween.scale(imageLeft.rectTransform, Vector3.one * 0.95f, 0.3f).setEaseOutBack();
    }

    public void SetPlatoIncorrect(bool eligioDerecha)
    {
        ColorTransition(colorIncorrecto);

        if (eligioDerecha)
            LeanTween.scale(imageRight.rectTransform, Vector3.one * 0.95f, 0.3f).setEaseOutBack();
        else
            LeanTween.scale(imageLeft.rectTransform, Vector3.one * 0.95f, 0.3f).setEaseOutBack();
    }

    public void ResetVisuals()
    {
        ColorTransition(colorNeutral);
        imageCentral.color = Color.white;
        LeanTween.scale(imageLeft.rectTransform, Vector3.one, 0.2f);
        LeanTween.scale(imageRight.rectTransform, Vector3.one, 0.2f);
    }

    private void ColorTransition(Color target)
    {
        panelCentral.GetComponent<Image>().color = target;
        imageCentral.color = target;
        panelIzquierdo.GetComponent<Image>().color = target;
        panelDerecho.GetComponent<Image>().color = target;
    }

    private void Pop(GameObject obj)
    {
        obj.transform.localScale = Vector3.one * 0.7f;
        LeanTween.scale(obj, Vector3.one, 0.4f).setEaseOutBack();
    }
    public void OnHoverEnter()
    {
        LeanTween.scale(gameObject, Vector3.one * 1.05f, 0.2f).setEaseOutBack();
    }

    public void OnHoverExit()
    {
        LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseOutBack();
    }

}
