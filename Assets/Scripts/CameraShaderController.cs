using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode] // „тобы шейдер работал и в редакторе, если нужно
public class CameraShaderController : MonoBehaviour
{
    private Material currentMaterial;

    /// <summary>
    /// ѕримен€ет новый материал (шейдер) к камере
    /// </summary>
    public void ApplyMaterial(Material mat)
    {
        currentMaterial = mat;
    }

    /// <summary>
    /// —брасывает шейдер (возвращает стандартное изображение)
    /// </summary>
    public void ClearMaterial()
    {
        currentMaterial = null;
    }

    // ћаги€ Unity: перехватываем рендер камеры и прогон€ем через шейдер
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (currentMaterial != null)
        {
            Graphics.Blit(src, dest, currentMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}