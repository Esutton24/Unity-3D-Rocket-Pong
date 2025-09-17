public static class ClassExtensions
{
    public static UnityEngine.Vector3 ToVector3(this UnityEngine.Vector2 v2)
    {
        return new UnityEngine.Vector3(v2.x, 0, v2.y);
    }
    public static UnityEngine.Vector3 Flatten(this UnityEngine.Vector3 v3)
    {
        v3.y = 0;
        return v3;
    }
    public static Direction ToDirection(this UnityEngine.Vector2 v2)
    {
        if (v2 == UnityEngine.Vector2.zero) return Direction.None;
        if (UnityEngine.Mathf.Abs(v2.x) > UnityEngine.Mathf.Abs(v2.y))
            return v2.x > 0 ? Direction.Right : Direction.Left;
        return v2.y > 0 ? Direction.Up : Direction.Down;
    }
    public static UnityEngine.Vector2 ToVector2(this Direction d)
    {
        switch (d)
        {
            case Direction.Left: return UnityEngine.Vector2.left;
            case Direction.Right: return UnityEngine.Vector2.right;
            case Direction.Up: return UnityEngine.Vector2.up;
            case Direction.Down: return UnityEngine.Vector2.down;
            default: return UnityEngine.Vector2.zero;
        }
    }

    public static int ToInt(this bool b) => b? 1 : 0;
    public static bool ToBool(this int i) => i != 0;
    
}
public enum Direction { Left, Right, Up, Down, None }
