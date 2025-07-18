using UnityEngine;

// Utility class 
public static class Util
{
    /// <returns>
    ///  -1 = not inside any sink corner  
    ///   0 = top-left  (x<0 , z>0)  
    ///   1 = top-right (x>0 , z>0)  
    ///   2 = bottom-right (x>0 , z<0)  
    ///   3 = bottom-left (x<0 , z<0)
    /// </returns>
    public static int GetSinkID(Vector3 p, float Limit)
    {
        if (Mathf.Abs(p.x) < Limit || Mathf.Abs(p.z) < Limit)
            return -1;                      // still inside play-area

        if (p.x < 0f && p.z > 0f) return 0;
        if (p.x > 0f && p.z > 0f) return 1;
        if (p.x > 0f && p.z < 0f) return 2;
        return 3;                           // p.x < 0 , p.z < 0
    }

    public static Vector3 XZ_to_XYZ(Vector2 xz)
    {
        return new Vector3(xz.x, 0f, xz.y);
    }

    public static Vector2 XYZ_to_XZ(Vector3 xyz)
    {
        return new Vector2(xyz.x, xyz.z);
    }
}
