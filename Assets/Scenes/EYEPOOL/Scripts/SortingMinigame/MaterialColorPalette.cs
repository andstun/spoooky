using UnityEngine;

// Scriptable object material palette to assign colours to portals (sinks) 

[CreateAssetMenu(fileName = "MaterialColorPalette", menuName = "Data/Material Color Palette")]
public class MaterialColorPalette : ScriptableObject
{
    [SerializeField] private Material[] materials;

    public Material[] GetMaterials()
    {
        return materials;
    }
    
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
