using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UIAnimator
{
    // Aplica una animación de clic al botón, simulando una pulsación (escalado con rebote)
    public static void AnimateButtonClick(Button button)
    {
        if (button == null) return;

        GameObject obj = button.gameObject;
        button.interactable = false; // Desactiva el botón durante la animación

        // Escala hacia abajo rápidamente y luego hacia arriba con rebote
        LeanTween.scale(obj, Vector3.one * 0.9f, 0.05f).setEaseInQuad().setOnComplete(() =>
        {
            LeanTween.scale(obj, Vector3.one, 0.1f).setEaseOutBack().setOnComplete(() =>
            {
                button.interactable = true; // Reactiva el botón al finalizar la animación
            });
        });
    }

    // Anima la aparición del botón con escala desde 0 hasta su tamaño normal
    public static void AnimateButtonAppear(Button button)
    {
        if (button == null) return;

        GameObject obj = button.gameObject;
        obj.transform.localScale = Vector3.zero; // Escala inicial oculta
        obj.SetActive(true); // Hace visible el botón
        LeanTween.scale(obj, Vector3.one, 0.25f).setEaseOutBack(); // Escala con rebote
    }

    // Anima un panel de ayuda con efecto pop-up (mostrar u ocultar)
    public static void AnimatePopUp(GameObject panel, bool show)
    {
        if (panel == null) return;

        if (show)
        {
            panel.transform.localScale = Vector3.zero; // Escala inicial al abrir
            panel.SetActive(true); // Activa el panel
            LeanTween.scale(panel, Vector3.one, 0.3f).setEaseOutBack(); // Animación de entrada
        }
        else
        {
            // Reduce escala a 0 y desactiva el panel al finalizar
            LeanTween.scale(panel, Vector3.zero, 0.2f).setEaseInBack().setOnComplete(() =>
            {
                panel.SetActive(false);
            });
        }
    }

    // Anima la entrada de una letra en un tile con un pequeño rebote
    public static void AnimateTileInput(GameObject tileObj)
    {
        if (tileObj == null) return;

        tileObj.transform.localScale = Vector3.one * 0.8f; // Disminuye la escala
        LeanTween.scale(tileObj, Vector3.one, 0.1f).setEaseOutBack(); // Escala hacia tamaño normal con rebote
    }

    // Anima el cambio de estado visual del tile, simulando un giro horizontal
    public static void AnimateTileFlip(GameObject tileObj, Image fill, Outline outline, Color fillColor, Color outlineColor)
    {
        if (tileObj == null || fill == null || outline == null) return;

        // Reduce la escala en el eje X para ocultar
        LeanTween.scaleX(tileObj, 0, 0.15f).setEaseInCubic().setOnComplete(() =>
        {
            // Aplica los nuevos colores mientras está oculto
            fill.color = fillColor;
            outline.effectColor = outlineColor;

            // Muestra el tile con escala X normal
            LeanTween.scaleX(tileObj, 1, 0.15f).setEaseOutCubic();
        });
    }

    // Anima las letras de un título una a una con rotación en eje Y
    public static void AnimateTextCharacters(TextMeshProUGUI textComponent, MonoBehaviour context)
    {
        context.StartCoroutine(AnimateCharactersCoroutine(textComponent));
    }

    // Corrutina que rota cada letra del título y la restaura a su estado original
    private static IEnumerator AnimateCharactersCoroutine(TextMeshProUGUI text)
    {
        text.ForceMeshUpdate(); // Asegura que el texto esté actualizado
        TMP_TextInfo textInfo = text.textInfo;

        // Backup completo de vértices
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        for (int i = 0; i < text.text.Length; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
            Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

            // Centro del carácter
            Vector3 center = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

            float duration = 0.1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float angle = Mathf.Lerp(0, 180, t);
                Quaternion rotation = Quaternion.Euler(0, angle, 0);

                for (int j = 0; j < 4; j++)
                {
                    destinationVertices[vertexIndex + j] = rotation * (sourceVertices[vertexIndex + j] - center) + center;
                }

                text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Restaurar posición original del carácter
            for (int j = 0; j < 4; j++)
            {
                destinationVertices[vertexIndex + j] = sourceVertices[vertexIndex + j];
            }

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            yield return new WaitForSeconds(0.03f);
        }

        // Restaurar completamente los datos originales para evitar distorsiones en futuras animaciones
        text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }


    public static void AnimateFinishBoardTiles(GameObject finishBoard, MonoBehaviour context)
    {
        context.StartCoroutine(AnimateFinishBoardTilesCoroutine(finishBoard));
    }

    private static IEnumerator AnimateFinishBoardTilesCoroutine(GameObject finishBoard)
    {
        TileSaludle[] tiles = finishBoard.GetComponentsInChildren<TileSaludle>();


        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tileObj = tiles[i].gameObject;

            // Primer giro: 0 → 180 grados en eje Y
            LeanTween.rotateY(tileObj, 180, 0.4f).setEaseInOutQuad();

            yield return new WaitForSeconds(0.1f); // Pequeño delay entre tiles
        }

        yield return new WaitForSeconds(0.6f); // Esperar a que terminen los giros

        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tileObj = tiles[i].gameObject;

            // Segundo giro: 180 → 0 grados
            LeanTween.rotateY(tileObj, 0, 0.4f).setEaseInOutQuad();

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.6f);

        // Ocultar todos los tiles al final (escala a 0 con rebote inverso)
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tileObj = tiles[i].gameObject;
            LeanTween.scale(tileObj, Vector3.zero, 0.3f).setEaseInBack();
        }
    }

    public static void HideWithScale(GameObject obj)
    {
        if (obj == null) return;

        LeanTween.scale(obj, Vector3.zero, 0.3f)
            .setEaseInBack()
            .setOnComplete(() =>
            {
                obj.SetActive(false);
                obj.transform.localScale = Vector3.one; // Restauramos para que esté listo para el siguiente uso
            });
    }




}
