using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Augmenta;  // Required for AugmentaObject

[RequireComponent(typeof(Collider))]
public class TunnelEntranceTrigger : MonoBehaviour
{
    private Collider triggerEntrance;
    [Header("Activation Zone (Tunnel) Reference")]
    public TunnelTrigger tunnel;

    void Awake()
    {
        triggerEntrance = GetComponent<Collider>();
        if (!triggerEntrance.isTrigger)
            triggerEntrance.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // tunnel.OnTriggerEnter(other);
    }
}
