using Augmenta;
using UnityEngine;

public class DetectPlayer : MonoBehaviour
{
    public AugmentaObject trackPlayer;
    public GameObject[] ghosts;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (trackPlayer == null && other.GetComponent<AugmentaObject>() != null)
        {
            trackPlayer = other.GetComponent<AugmentaObject>();
            Instantiate(ghosts[Random.Range(0, ghosts.Length)], new Vector3(transform.position.x, 2.94f, 22.94f), Quaternion.identity);
        }
        else
        {
            return;
        }
    }

    public float GetPlayerX()
    {
        return trackPlayer.GetComponent<Transform>().position.x;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<AugmentaObject>() == trackPlayer)
        {
            trackPlayer = null;
        }
    }
}
