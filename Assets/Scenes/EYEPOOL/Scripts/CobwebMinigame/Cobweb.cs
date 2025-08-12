using UnityEngine;

public class Cobweb : MonoBehaviour
{
    [SerializeField] private TYPE cobwebMeshPath; // TODO read from defined folder. 

    void Awake()
    {
        // particles should take the path of the cobwebMeshPath. 
        // maybe they should flicker
        // in general they should not have a lifetime, they should stay until 
            // on that note, it should probably look alive somehow; like it's swaying in the wind. 
    }

    void Start()
    {

    }

    void Update()
    {

    }
}