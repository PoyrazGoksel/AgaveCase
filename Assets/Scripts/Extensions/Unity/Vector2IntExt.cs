using UnityEngine;

namespace Extensions.Unity
{
    public static class Vector2IntExt
    {
        public static int Mag(this Vector2Int vector2Int)
        {
            return Mathf.Abs(vector2Int.x) + Mathf.Abs(vector2Int.y);
        }
    }
}