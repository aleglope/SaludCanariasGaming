using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
public class PlatoImporter : EditorWindow
{
    [MenuItem("Herramientas/Importar Platos desde JSON")]
    public static void ShowWindow()
    {
        string path = EditorUtility.OpenFilePanel("Selecciona archivo JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        PlatoJSON[] platos = JsonHelper.FromJson<PlatoJSON>(json);

        foreach (var plato in platos)
        {
            CrearPlatoScriptable(plato);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Importación completada.");
    }

    private static void CrearPlatoScriptable(PlatoJSON data)
    {
        PlatoData asset = ScriptableObject.CreateInstance<PlatoData>();
        asset.nombre = data.nombre;
        asset.origen = data.origen;
        asset.caloriasPorRacion = data.caloriasPorRacion;
        asset.descripcion = data.descripcion;
        asset.beneficio = data.beneficio;

        // Cargar imagen del Resources
        Sprite sprite = Resources.Load<Sprite>("Platos/" + data.imagen);
        if (sprite == null)
        {
            Debug.LogWarning($"[PlatoImporter] No se encontró la imagen '{data.imagen}', se usará 'default'.");
            sprite = Resources.Load<Sprite>("Platos/default");
        }
        asset.imagen = sprite;

        // Crear nombre seguro del archivo
        string safeName = string.Concat(data.nombre.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
            .Replace(" ", "_");

        string folderPath = "Assets/ScriptableObjects/Platos";
        string filePath = $"{folderPath}/{safeName}.asset";

        Directory.CreateDirectory(folderPath);

        if (File.Exists(filePath))
        {
            Debug.Log($"[PlatoImporter] Ya existe: {filePath}, omitiendo.");
            return;
        }

        AssetDatabase.CreateAsset(asset, filePath);
    }

    [System.Serializable]
    public class PlatoJSON
    {
        public string nombre;
        public string origen;
        public int caloriasPorRacion;
        public string descripcion;
        public string imagen;
        public string beneficio;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string wrapped = "{\"array\":" + json + "}";
            return JsonUtility.FromJson<Wrapper<T>>(wrapped).array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
