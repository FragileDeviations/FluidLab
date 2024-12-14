using System;

using System.Numerics;

namespace FluidLab;

public static class MathUtilities
{
    public const float Rad2Deg = (float)(360f / (Math.PI * 2f));

    public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
    {
        return RotateTowards(LookRotation(fromDirection), LookRotation(toDirection), float.MaxValue);
    }

    public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
    {
        float num = Angle(from, to);
        if (num == 0f)
        {
            return to;
        }
        float t = Math.Min(1f, maxDegreesDelta / num);
        return Quaternion.Slerp(from, to, t);
    }

    public static Quaternion LookRotation(Vector3 forward)
    {
        Vector3 up = Vector3.UnitY;
        forward = Vector3.Normalize(forward);
        Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
        up = Vector3.Cross(forward, right);
        var m00 = right.X;
        var m01 = right.Y;
        var m02 = right.Z;
        var m10 = up.X;
        var m11 = up.Y;
        var m12 = up.Z;
        var m20 = forward.X;
        var m21 = forward.Y;
        var m22 = forward.Z;


        float num8 = (m00 + m11) + m22;
        var quaternion = new Quaternion();
        if (num8 > 0f)
        {
            var num = (float)Math.Sqrt(num8 + 1f);
            quaternion.W = num * 0.5f;
            num = 0.5f / num;
            quaternion.X = (m12 - m21) * num;
            quaternion.Y = (m20 - m02) * num;
            quaternion.Z = (m01 - m10) * num;
            return quaternion;
        }
        if ((m00 >= m11) && (m00 >= m22))
        {
            var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
            var num4 = 0.5f / num7;
            quaternion.X = 0.5f * num7;
            quaternion.Y = (m01 + m10) * num4;
            quaternion.Z = (m02 + m20) * num4;
            quaternion.W = (m12 - m21) * num4;
            return quaternion;
        }
        if (m11 > m22)
        {
            var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
            var num3 = 0.5f / num6;
            quaternion.X = (m10 + m01) * num3;
            quaternion.Y = 0.5f * num6;
            quaternion.Z = (m21 + m12) * num3;
            quaternion.W = (m20 - m02) * num3;
            return quaternion;
        }
        var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
        var num2 = 0.5f / num5;
        quaternion.X = (m20 + m02) * num2;
        quaternion.Y = (m21 + m12) * num2;
        quaternion.Z = 0.5f * num5;
        quaternion.W = (m01 - m10) * num2;
        return quaternion;
    }

    public static float Angle(Quaternion a, Quaternion b)
    {
        float f = Quaternion.Dot(a, b);
        return (float)Math.Acos(Math.Min(Math.Abs(f), 1f)) * 2f * Rad2Deg;
    }
}
