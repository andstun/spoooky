using UnityEngine;

public class GhostPuddle : MonoBehaviour
{
    public Color splatPrefabColor;
    private float startTime;
    private float duration = 1f;
    void Awake()
    {
        splatPrefabColor = gameObject.GetComponent<SpriteRenderer>().color;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float elapsed = (Time.time - startTime) / duration;
        gameObject.GetComponent<SpriteRenderer>().color = new Color(splatPrefabColor.r, splatPrefabColor.g, splatPrefabColor.b, Mathf.Lerp(splatPrefabColor.a, 0, elapsed));
        if (splatPrefabColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
