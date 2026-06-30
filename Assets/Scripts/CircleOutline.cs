using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class CircleOutline : MaskableGraphic
{
    [SerializeField] private float thickness = 4f;     // Толщина кольца в пикселях
    [SerializeField] private int segments = 64;        // Количество сегментов (чем больше, тем гладче)

    public float Thickness
    {
        get => thickness;
        set { thickness = value; SetVerticesDirty(); }
    }

    public int Segments
    {
        get => segments;
        set { segments = Mathf.Max(3, value); SetVerticesDirty(); }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;
        float centerX = rect.width * 0.5f;
        float centerY = rect.height * 0.5f;

        // Радиусы: внешний и внутренний
        float outerRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
        float innerRadius = outerRadius - thickness;
        if (innerRadius < 0) innerRadius = 0;

        // Угол между сегментами
        float angleStep = 360f / segments;
        float radStep = angleStep * Mathf.Deg2Rad;

        Color32 c = color;

        // Генерируем вершины кольца
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * radStep;
            float angle2 = (i + 1) * radStep;

            // Внешние точки
            float outerX1 = centerX + Mathf.Cos(angle1) * outerRadius;
            float outerY1 = centerY + Mathf.Sin(angle1) * outerRadius;
            float outerX2 = centerX + Mathf.Cos(angle2) * outerRadius;
            float outerY2 = centerY + Mathf.Sin(angle2) * outerRadius;

            // Внутренние точки
            float innerX1 = centerX + Mathf.Cos(angle1) * innerRadius;
            float innerY1 = centerY + Mathf.Sin(angle1) * innerRadius;
            float innerX2 = centerX + Mathf.Cos(angle2) * innerRadius;
            float innerY2 = centerY + Mathf.Sin(angle2) * innerRadius;

            // Смещение в локальные координаты (от центра RectTransform)
            int startVertex = vh.currentVertCount;

            vh.AddVert(new Vector3(outerX1 - centerX, outerY1 - centerY), c, Vector4.zero);
            vh.AddVert(new Vector3(outerX2 - centerX, outerY2 - centerY), c, Vector4.zero);
            vh.AddVert(new Vector3(innerX2 - centerX, innerY2 - centerY), c, Vector4.zero);
            vh.AddVert(new Vector3(innerX1 - centerX, innerY1 - centerY), c, Vector4.zero);

            // Два треугольника на сегмент
            vh.AddTriangle(startVertex, startVertex + 1, startVertex + 2);
            vh.AddTriangle(startVertex, startVertex + 2, startVertex + 3);
        }
    }
}