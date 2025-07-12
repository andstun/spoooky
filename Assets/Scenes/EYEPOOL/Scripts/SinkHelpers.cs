using UnityEngine;

public static class SinkHelpers
{
    // square extends from -16.8 … +16.8 on X & Z
    private const float Limit = 15.6f;

    /// <returns>
    ///  -1 = not inside any sink corner  
    ///   0 = top-left  (x<0 , z>0)  
    ///   1 = top-right (x>0 , z>0)  
    ///   2 = bottom-right (x>0 , z<0)  
    ///   3 = bottom-left (x<0 , z<0)
    /// </returns>
    public static int GetSinkID(Vector3 p)
    {
        Debug.Log($"x-coords: {p.x} z-coords: {p.z}");
        if (Mathf.Abs(p.x) < Limit || Mathf.Abs(p.z) < Limit)
            return -1;                      // still inside play-area

        if (p.x < 0f && p.z > 0f) return 0;
        if (p.x > 0f && p.z > 0f) return 1;
        if (p.x > 0f && p.z < 0f) return 2;
        return 3;                           // p.x < 0 , p.z < 0
    }
}
