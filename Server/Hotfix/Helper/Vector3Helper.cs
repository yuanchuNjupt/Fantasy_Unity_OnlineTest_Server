using Fantasy;
using Fantasy.Lobby;

namespace Hotfix.Helper;

public static class Vector3Helper
{
    public static CSVector3 ToCSVector3(this Vector3 v)
    {
        return new CSVector3()
        {
            x = v.x,
            y = v.y,
            z = v.z
        };
    }
    
    public static Vector3 ToVector3(this CSVector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
}