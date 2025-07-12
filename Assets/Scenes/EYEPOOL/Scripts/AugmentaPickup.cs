using UnityEngine;

/// <summary>
/// Simple trigger-based “hand” for an Augmenta person.
/// One orb at a time; drops it automatically when the orb enters *any* sink.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AugmentaPickup : MonoBehaviour
{
    private Orb carriedOrb;

    // assure trigger mode
    private void Awake()
    {
        // Debug.Log("I am awake");
        var col = GetComponent<Collider>();
    }

    // ─────────────────────────────────────────────── Pick up
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered collider trigger between box and something");
        if (carriedOrb != null) return;                 // already holding one

        Orb orb = other.GetComponent<Orb>();
        if (orb != null && !orb.IsAttached)
        {
            carriedOrb = orb;
            carriedOrb.AttachTo(transform);
        }
    }

    // ─────────────────────────────────────────────── Follow + drop logic
    private void Update()
    {
        // Debug.Log("Updating the frame for augmenta pickup");
        if (carriedOrb == null) return;

        int sinkHere = SinkHelpers.GetSinkID(carriedOrb.transform.position);

        if (sinkHere == carriedOrb.targetSinkID)
        {
            // correct sink reached → orb destroys & spawns replacement
            carriedOrb.Detach(true);
            carriedOrb = null;
        }
        else if (sinkHere != -1)
        {
            // wrong sink corner: just drop it on the floor
            carriedOrb.Detach(false);
            carriedOrb = null;
        }
    }
}
