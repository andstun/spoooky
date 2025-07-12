using UnityEngine;
using Augmenta;

[RequireComponent(typeof(AugmentaObject))]
public class AO_FollowCentroid : MonoBehaviour
{
    AugmentaObject ao;
    void Awake() => ao = GetComponent<AugmentaObject>(); // fetches the object data (streamed in realtime)

    void LateUpdate()  // sets the objects transform coords to the 3D world pos
    {
        transform.localPosition = ao.worldPosition3D;
    }
}
