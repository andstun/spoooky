using UnityEngine;

public class Orb : MonoBehaviour
{
    public int targetSinkID; // The correct sink this orb belongs to

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Check if it hit another Orb
        if (other.TryGetComponent<Orb>(out var otherOrb))
        {
            HandleOrbCollision(otherOrb);
        }

        // Check if it hit a Sink
        else if (other.TryGetComponent<Sink>(out var sink))
        {
            HandleSinkCollision(sink);
        }
    }

    void HandleOrbCollision(Orb otherOrb)
    {
        Debug.Log($"{name} collided with another orb: {otherOrb.name}");
        // Trigger your specific orb-orb interaction here
    }

    void HandleSinkCollision(Sink sink)
    {
        if (sink.sinkID == targetSinkID)
        {
            Debug.Log($"{name} entered the correct sink (ID {sink.sinkID})");
            // Correct sink logic
        }
        else
        {
            Debug.Log($"{name} collided with wrong sink (ID {sink.sinkID})");
            // Ignore or bounce off
        }
    }
}
