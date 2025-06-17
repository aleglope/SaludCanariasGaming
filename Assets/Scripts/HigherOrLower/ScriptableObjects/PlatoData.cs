using UnityEngine;

[CreateAssetMenu(fileName = "NuevoPlato", menuName = "Datos/Plato Típico")]
public class PlatoData : ScriptableObject
{
    [Header("Información del Plato")]
    public string nombre;
    public string origen; // España o Canarias
    public int caloriasPorRacion;

    [TextArea]
    public string descripcion;

    [TextArea]
    public string beneficio;

    [Header("Imagen")]
    public Sprite imagen;
}
