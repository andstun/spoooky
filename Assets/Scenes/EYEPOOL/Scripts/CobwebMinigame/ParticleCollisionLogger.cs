using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleCollisionLogger : MonoBehaviour
{
    private ParticleSystem ps;
    private readonly List<ParticleCollisionEvent> eventsBuf = new List<ParticleCollisionEvent>(64);

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Fires once per collider object that was hit this frame
    private void OnParticleCollision(GameObject other)
    {
        int n = ps.GetCollisionEvents(other, eventsBuf);
        Debug.Log($"Particles from [{name}] hit [{other.name}] — {n} events");

        // Draw a few impact points/normals so you can see them in Scene view
        int limit = Mathf.Min(n, 10);
        for (int i = 0; i < limit; i++)
        {
            var e = eventsBuf[i];
            Debug.DrawRay(e.intersection, e.normal * 0.5f, Color.red, 0.25f, false);
        }
        eventsBuf.Clear();
    }
}
