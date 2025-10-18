using System.Collections.Generic;
using UnityEngine;
using Augmenta;

public class FigureOpacity : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] sprites;
    [SerializeField] private float fadeSpeed = 0.5f;
    public HashSet<AugmentaObject> usersInZone = new();


    void Update()
    {
        if(usersInZone.Count > 0 && sprites[0].color.a < 1f)
        {
            // Fade In sprites
            foreach (SpriteRenderer sr in sprites)
            {
                Color c = sr.color;
                c.a = Mathf.MoveTowards(c.a, 1f, fadeSpeed * Time.deltaTime);
                sr.color = c;
            }
        }
        else if(usersInZone.Count == 0 && sprites[0].color.a > 0f)
        {
            // Fade in sprites
            foreach (SpriteRenderer sr in sprites)
            {
                Color c = sr.color;
                c.a = Mathf.MoveTowards(c.a, 0f, fadeSpeed * Time.deltaTime);
                sr.color = c;
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        AugmentaObject augmenta = other.GetComponent<AugmentaObject>();
        if (augmenta == null)
        {
            return;
        }
        else
        {
            
            usersInZone.Add(augmenta);
        }
    }

    void OnTriggerExit(Collider other)
    {
        AugmentaObject augmenta = other.GetComponent<AugmentaObject>();
        if (augmenta == null)
        {
            return;
        }
        else
        {
            usersInZone.Remove(augmenta);
        }
    }
}
