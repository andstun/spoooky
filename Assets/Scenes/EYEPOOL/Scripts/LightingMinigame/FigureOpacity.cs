using UnityEngine;
using Augmenta;
using UnityEngine.AI;

public class FigureOpacity : MonoBehaviour
{
    [SerializeField] bool triggerOpacity = false;

    [SerializeField] private Material figureMaterial;

    [SerializeField] private Color materialColor;



    void Awake()
    {
        materialColor = figureMaterial.GetColor("_GlowColor");
    }

    void Update()
    {

        if (triggerOpacity)
        {
            if (figureMaterial.GetColor("_GlowColor").a < 255)
            {
                Debug.Log("Appearing");
                materialColor.a += 0.01f;
                figureMaterial.SetColor("_GlowColor", materialColor);
            }

        }
        else if(!triggerOpacity && figureMaterial.GetColor("_GlowColor").a > 0)
        {
            Debug.Log("Fading");
            materialColor.a -= 0.01f;
            figureMaterial.SetColor("_GlowColor", materialColor);
        }
        

    }
    void OnTriggerStay(Collider other)
    {
        AugmentaObject augmenta = other.GetComponent<AugmentaObject>();
        if (augmenta == null)
        {
            return;
        }
        triggerOpacity = true;
    }

    void OnTriggerExit(Collider other)
    {
        AugmentaObject augmenta = other.GetComponent<AugmentaObject>();
        if (augmenta == null)
        {
            return;
        }
        triggerOpacity = false;
    }
}
