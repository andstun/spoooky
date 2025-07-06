using UnityEngine; // Physics Engine and other Unity essentials 
using System.Collections;
using UnityEditor;

public class OrbSpawner : MonoBehaviour
{
    public int orbsLeft = 10;
    private enum Colours
    {
        Red,
        Green,
        Blue,
        Yellow
    };

    private Color[] colours = {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };

    private GameObject[] orbs = { };
    void Start()
    {
        // not sure if any initial generation should happen
        // i think skip to update makes sense
    }

    void Update()
    {
        // if (orbsLeft)
        // {

        // }
    }

    void SpawnOrb()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = GenerateOrbPosition();
        Renderer rend = sphere.GetComponent<Renderer>();
        rend.material.color = GenerateOrbColour();
        sphere.AddComponent<Rigidbody>();
        sphere.AddComponent<Orb>();
    }

    void DeleteOrb()
    { // called when 

    }

    Vector3 GenerateOrbPosition()
    {
        float x = Random.Range(-13.9f, 13.9f);
        float z = Random.Range(-13.9f, 13.9f);

        return new Vector3(x, 0.0f, z);
    }

    Color GenerateOrbColour()
    { // colour sampler
        int colourNum = Random.Range(0, 4);
        return colours[colourNum];
    }
}