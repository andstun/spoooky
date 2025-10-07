using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialPrefabPalette", menuName = "Data/Material → Prefab Palette")]
public class GhostPalette : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string name;              // optional label for readability
        public Material material;        // the sink/portal material
        public Sprite captureSprite; // the material to use when the ghost is captured
        public GameObject prefab;        // the ghost prefab to spawn
        public GameObject splat;         // the ghost splat animations
    }

    [SerializeField] private Entry[] entries;
    [SerializeField] private GameObject defaultPrefab;   // fallback if no match

    // Optional: keep your material/color helpers
    public Entry[] GetEntries()
    {
        return entries;
    }
    public Material[] GetMaterials()
    {
        var arr = new Material[entries.Length];
        for (int i = 0; i < entries.Length; i++) arr[i] = entries[i].material;
        return arr;
    }

    public Color[] GetColors()
    {
        var arr = new Color[entries.Length];
        for (int i = 0; i < entries.Length; i++) arr[i] = entries[i].material != null ? entries[i].material.color : Color.magenta;
        return arr;
    }

    public GameObject[] GetPrefabs()
    {
        var arr = new GameObject[entries.Length];
        for (int i = 0; i < entries.Length; i++) arr[i] = entries[i].prefab;
        return arr;
    }

    public GameObject[] GetSplats()
    {
        var arr = new GameObject[entries.Length];
        for (int i = 0; i < entries.Length; i++) arr[i] = entries[i].splat;
        return arr;
    }

    // ---- Lookups ----
    // Fast material reference match.
    public GameObject GetPrefabForMaterial(Material mat)
    {
        if (mat == null) return defaultPrefab;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].material == mat) return entries[i].prefab;
        }
        return defaultPrefab;
    }

    // Color match with tolerance (useful if you only know the color, not the exact Material instance).
    public GameObject GetPrefabForColor(Color color, float tolerance = 0.01f)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var m = entries[i].material;
            if (m == null) continue;
            if (Approximately(m.color, color, tolerance)) return entries[i].prefab;
        }
        return defaultPrefab;
    }

    private static bool Approximately(Color a, Color b, float eps)
    {
        return Mathf.Abs(a.r - b.r) <= eps &&
               Mathf.Abs(a.g - b.g) <= eps &&
               Mathf.Abs(a.b - b.b) <= eps &&
               Mathf.Abs(a.a - b.a) <= eps;
    }
}
