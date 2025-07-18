using UnityEngine;

[CreateAssetMenu(fileName = "MaterialColorPalette", menuName = "Data/Material Color Palette")]
public class MaterialColorPalette : ScriptableObject
{
    public Material[] materials;

    public Color[] GetColors()
    {
        Color[] colors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            colors[i] = materials[i].color;
        }
        return colors;
    }
}
