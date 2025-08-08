using UnityEngine;
using System;

// Miscellaneous Utility class 
public static class Util
{
    ///  -1 = not inside any sink corner  
    ///   0 = top-left  (x<0 , z>0)  
    ///   1 = top-right (x>0 , z>0)  
    ///   2 = bottom-right (x>0 , z<0)  
    ///   3 = bottom-left (x<0 , z<0)
    public static int GetSinkID(Vector3 p, float Limit)
    {
        if (Mathf.Abs(p.x) < Limit || Mathf.Abs(p.z) < Limit)
            return -1;                      // still inside play area

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

    public static Vector2 GetExtents(Vector2 xRange, Vector2 zRange)
    {
        return new Vector2(xRange.y - xRange.x, zRange.y - zRange.x);
    }

    public static class RandomExtensions // allow for randomly-generated values following a normal distribution
    {
        private static bool hasSpare = false;
        private static float spare;

        public static float Gaussian(float mean = 0, float stdDev = 1)
        {
            if (hasSpare)
            {
                hasSpare = false;
                return spare * stdDev + mean;
            }

            float u, v, s;
            do
            {
                u = UnityEngine.Random.value * 2 - 1;
                v = UnityEngine.Random.value * 2 - 1;
                s = u * u + v * v;
            } while (s >= 1 || s == 0);

            s = Mathf.Sqrt(-2.0f * Mathf.Log(s) / s);
            spare = v * s;
            hasSpare = true;
            return mean + stdDev * u * s;
        }
    }

    public static int[] GetShuffledArray(int N)
    {
        int[] randomOrder = new int[N];

        for (int i = 0; i < N; i++) randomOrder[i] = i;

        System.Random rng = new System.Random();
        for (int i = 0; i < N; i++)
        {
            int j = rng.Next(i + 1); // randomly generate swap index
            (randomOrder[i], randomOrder[j]) = (randomOrder[j], randomOrder[i]); // std::swap, basically
        }
        return randomOrder;
    }
}
