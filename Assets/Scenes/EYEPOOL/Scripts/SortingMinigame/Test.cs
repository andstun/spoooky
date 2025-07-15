using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	public float radius = 1;
	public Vector2 regionSize = Vector2.one;
	public int rejectionSamples = 30;
	public float displayRadius =1;

	List<Vector2> points;

	void OnValidate() {
		points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
	}

	void OnDrawGizmos() {
        Gizmos.DrawWireCube(new Vector3(regionSize.x / 2, 0f, regionSize.y / 2), new Vector3(regionSize.x, 0f, regionSize.y));

		if (points != null) {
			foreach (Vector2 point in points) {
				Gizmos.DrawSphere(new Vector3(point.x, 0f, point.y), displayRadius);
			}
		}
	}
}