using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private DetectPlayer detectPlayer;
    void Awake()
    {
        detectPlayer = FindAnyObjectByType<DetectPlayer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (detectPlayer.trackPlayer != null)
        {
            TransformToPlayerX();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void TransformToPlayerX()
    {
        Vector3 pos = transform.position;
        pos.x = detectPlayer.GetPlayerX();
        transform.position = pos;
    }
    
}
