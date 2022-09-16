using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox;
public enum EasingType
{
    None = -1,
    Linear = 0,
    SineIn, SineOut, SineInOut,
    QuadIn, QuadOut, QuadInOut,
    CubicIn, CubicOut, CubicInOut,
    QuartIn, QuartOut, QuartInOut,
    QuintIn, QuintOut, QuintInOut,
    ExpoIn, ExpoOut, ExpoInOut,
    ExtremeIn, ExtremeOut, ExtremeInOut,
    ElasticIn, ElasticOut, ElasticInOut,
    ElasticSoftIn, ElasticSoftOut, ElasticSoftInOut,
    BackIn, BackOut, BackInOut,
    BounceIn, BounceOut, BounceInOut
};

public static class Utils
{
    public const float Deg2Rad = MathF.PI / 180f;
    public const float Rad2Deg = 360f / (MathF.PI * 2f);

    public static Vector2 RotateVector(Vector2 v, float degrees)
    {
        var rads = Deg2Rad * degrees;
        var ca = MathF.Cos(rads);
        var sa = MathF.Sin(rads);

        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }

    public static float FastSin(float input)
    {
        // wrap input angle to -PI..PI
        while (input < -3.14159265f)
            input += 6.28318531f;
        while (input > 3.14159265f)
            input -= 6.28318531f;

        return 1.27323954f * input + 0.405284735f * (input < 0f ? 1f : -1f) * input * input;
    }

    public static float DynamicEaseTo(float current, float goal, float factorPercent, float dt, float referenceFrameRate = 60f)
    {
        if (float.IsPositiveInfinity(dt)) 
            return goal;

        return current + (goal - current) * (1f - MathF.Pow(1f - MathX.Clamp(factorPercent, 0f, 1f), dt * referenceFrameRate));
    }

    public static Vector2 DynamicEaseTo(Vector2 current, Vector2 goal, float factorPercent, float dt, float referenceFrameRate = 60f)
    {
        if (float.IsPositiveInfinity(dt))
            return goal;
        var diff = goal - current;

        return current + diff * (1f - MathF.Pow(1f - MathX.Clamp(factorPercent, 0f, 1f), dt * referenceFrameRate));
    }

    public static float EaseUnclamped(float value, EasingType easingType)
    {
        switch (easingType)
        {
            case EasingType.SineIn: return SineIn(value);
            case EasingType.SineOut: return SineOut(value);
            case EasingType.SineInOut: return SineInOut(value);

            case EasingType.QuadIn: return QuadIn(value);
            case EasingType.QuadOut: return QuadOut(value);
            case EasingType.QuadInOut: return QuadInOut(value);

            case EasingType.CubicIn: return CubicIn(value);
            case EasingType.CubicOut: return CubicOut(value);
            case EasingType.CubicInOut: return CubicInOut(value);

            case EasingType.QuartIn: return QuartIn(value);
            case EasingType.QuartOut: return QuartOut(value);
            case EasingType.QuartInOut: return QuartInOut(value);

            case EasingType.QuintIn: return QuintIn(value);
            case EasingType.QuintOut: return QuintOut(value);
            case EasingType.QuintInOut: return QuintInOut(value);

            case EasingType.ExpoIn: return ExpoIn(value);
            case EasingType.ExpoOut: return ExpoOut(value);
            case EasingType.ExpoInOut: return ExpoInOut(value);

            case EasingType.ExtremeIn: return ExtremeIn(value);
            case EasingType.ExtremeOut: return ExtremeOut(value);
            case EasingType.ExtremeInOut: return ExtremeInOut(value);

            case EasingType.ElasticIn: return ElasticIn(value);
            case EasingType.ElasticOut: return ElasticOut(value);
            case EasingType.ElasticInOut: return ElasticInOut(value);

            case EasingType.ElasticSoftIn: return ElasticSoftIn(value);
            case EasingType.ElasticSoftOut: return ElasticSoftOut(value);
            case EasingType.ElasticSoftInOut: return ElasticSoftInOut(value);

            case EasingType.BackIn: return BackIn(value);
            case EasingType.BackOut: return BackOut(value);
            case EasingType.BackInOut: return BackInOut(value);

            case EasingType.BounceIn: return BounceIn(value);
            case EasingType.BounceOut: return BounceOut(value);
            case EasingType.BounceInOut: return BounceInOut(value);
            default: return value;
        }
    }

    public static float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax, EasingType easingType = EasingType.Linear, bool clamp = true)
    {
        if (inputMin.Equals(inputMax) || outputMin.Equals(outputMax))
            return outputMin;

        //            if (inputMin.Equals(inputMax) || outputMin.Equals(outputMax))
        //                return outputMax;

        if (clamp)
        {
            // clamp input
            if (inputMax > inputMin)
            {
                if (value < inputMin) value = inputMin;
                else if (value > inputMax) value = inputMax;
            }
            else if (inputMax < inputMin)
            {
                if (value > inputMin) value = inputMin;
                else if (value < inputMax) value = inputMax;
            }
        }

        var ratio = EaseUnclamped((value - inputMin) / (inputMax - inputMin), easingType);

        var outVal = outputMin + ratio * (outputMax - outputMin);

        //            // clamp output
        //            if (outputMax < outputMin) {
        //                if (outVal < outputMax) outVal = outputMax;
        //                else if (outVal > outputMin) outVal = outputMin;
        //            } else {
        //                if (outVal > outputMax) outVal = outputMax;
        //                else if (outVal < outputMin) outVal = outputMin;
        //            }

        return outVal;
    }

    public static float MapReturn(float value, float inputMin, float inputMax, float outputMin, float outputMax, EasingType easingType)
    {
        var halfway = inputMin + (inputMax - inputMin) * 0.5f;
        if (value < halfway) return Map(value, inputMin, halfway, outputMin, outputMax, easingType);
        else return Map(value, halfway, inputMax, outputMax, outputMin, GetOppositeEasingType(easingType));
    }

    public static float EasePercent(float percent, EasingType easingType)
    {
        return Map(percent, 0f, 1f, 0f, 1f, easingType);
    }

    public static EasingType GetOppositeEasingType(EasingType easingType)
    {
        var opposite = EasingType.Linear;
        switch (easingType)
        {
            case EasingType.SineIn: opposite = EasingType.SineOut; break;
            case EasingType.SineOut: opposite = EasingType.SineIn; break;
            case EasingType.SineInOut: opposite = EasingType.SineInOut; break;

            case EasingType.QuadIn: opposite = EasingType.QuadOut; break;
            case EasingType.QuadOut: opposite = EasingType.QuadIn; break;
            case EasingType.QuadInOut: opposite = EasingType.QuadInOut; break;

            case EasingType.CubicIn: opposite = EasingType.CubicOut; break;
            case EasingType.CubicOut: opposite = EasingType.CubicIn; break;
            case EasingType.CubicInOut: opposite = EasingType.CubicInOut; break;

            case EasingType.QuartIn: opposite = EasingType.QuartOut; break;
            case EasingType.QuartOut: opposite = EasingType.QuartIn; break;
            case EasingType.QuartInOut: opposite = EasingType.QuartInOut; break;

            case EasingType.QuintIn: opposite = EasingType.QuintOut; break;
            case EasingType.QuintOut: opposite = EasingType.QuintIn; break;
            case EasingType.QuintInOut: opposite = EasingType.QuintInOut; break;

            case EasingType.ExpoIn: opposite = EasingType.ExpoOut; break;
            case EasingType.ExpoOut: opposite = EasingType.ExpoIn; ; break;
            case EasingType.ExpoInOut: opposite = EasingType.ExpoInOut; break;

            case EasingType.ExtremeIn: opposite = EasingType.ExtremeOut; break;
            case EasingType.ExtremeOut: opposite = EasingType.ExtremeIn; break;
            case EasingType.ExtremeInOut: opposite = EasingType.ExtremeInOut; break;

            case EasingType.ElasticIn: opposite = EasingType.ElasticOut; break;
            case EasingType.ElasticOut: opposite = EasingType.ElasticIn; break;
            case EasingType.ElasticInOut: opposite = EasingType.ElasticInOut; break;

            case EasingType.ElasticSoftIn: opposite = EasingType.ElasticSoftOut; break;
            case EasingType.ElasticSoftOut: opposite = EasingType.ElasticSoftIn; break;
            case EasingType.ElasticSoftInOut: opposite = EasingType.ElasticSoftInOut; break;

            case EasingType.BackIn: opposite = EasingType.BackOut; break;
            case EasingType.BackOut: opposite = EasingType.BackIn; break;
            case EasingType.BackInOut: opposite = EasingType.BackInOut; break;

            case EasingType.BounceIn: opposite = EasingType.BounceOut; break;
            case EasingType.BounceOut: opposite = EasingType.BounceIn; break;
            case EasingType.BounceInOut: opposite = EasingType.BounceInOut; break;
        }

        return opposite;
    }

    public static float SineIn(float t) { return 1f - MathF.Cos(t * MathF.PI * 0.5f); }
    public static float SineOut(float t) { return MathF.Sin(t * (MathF.PI * 0.5f)); }
    public static float SineInOut(float t) { return -0.5f * (MathF.Cos(MathF.PI * t) - 1f); }

    public static float QuadIn(float t) { return t * t; }
    public static float QuadOut(float t) { return t * (2f - t); }
    public static float QuadInOut(float t) { return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t; }

    public static float CubicIn(float t) { return t * t * t; }
    public static float CubicOut(float t) { var t1 = t - 1f; return t1 * t1 * t1 + 1f; }
    public static float CubicInOut(float t) { return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f; }

    public static float QuartIn(float t) { return t * t * t * t; }
    public static float QuartOut(float t) { var t1 = t - 1f; return 1f - t1 * t1 * t1 * t1; }
    public static float QuartInOut(float t) { return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f; }

    public static float QuintIn(float t) { return t * t * t * t * t; }
    public static float QuintOut(float t) { var t1 = t - 1f; return 1f + t1 * t1 * t1 * t1 * t1; }
    public static float QuintInOut(float t) { var t1 = t - 1f; return t < 0.5f ? 16f * t * t * t * t * t : 1f + 16f * t1 * t1 * t1 * t1 * t1; }

    public static float ExpoIn(float t) { return MathF.Pow(2f, 10f * (t - 1f)); }
    public static float ExpoOut(float t) { return 1f - MathF.Pow(2f, -10f * t); }
    public static float ExpoInOut(float t) { return t < 0.5f ? ExpoIn(t * 2f) * 0.5f : 1f - ExpoIn(2f - t * 2f) * 0.5f; }

    public static float ExtremeIn(float t) { return MathF.Pow(10f, 10f * (t - 1f)); }
    public static float ExtremeOut(float t) { return 1f - MathF.Pow(10f, -10f * t); }
    public static float ExtremeInOut(float t) { return t < 0.5f ? ExtremeIn(t * 2f) * 0.5f : 1f - ExtremeIn(2f - t * 2f) * 0.5f; }

    public static float ElasticIn(float t) { return 1f - ElasticOut(1f - t); }
    public static float ElasticOut(float t) { var p = 0.3f; return MathF.Pow(2f, -10f * t) * MathF.Sin((t - p / 4f) * (2f * (float)Math.PI) / p) + 1f; }
    public static float ElasticInOut(float t) { return t < 0.5f ? ElasticIn(t * 2f) * 0.5f : 1f - ElasticIn(2f - t * 2f) * 0.5f; }

    public static float ElasticSoftIn(float t) { return 1f - ElasticSoftOut(1f - t); }
    public static float ElasticSoftOut(float t) { var p = 0.5f; return MathF.Pow(2f, -10f * t) * MathF.Sin((t - p / 4f) * (2f * (float)Math.PI) / p) + 1f; }
    public static float ElasticSoftInOut(float t) { return t < 0.5f ? ElasticSoftIn(t * 2f) * 0.5f : 1f - ElasticSoftIn(2f - t * 2f) * 0.5f; }

    public static float BackIn(float t) { var p = 1f; return t * t * ((p + 1f) * t - p); }
    public static float BackOut(float t) { var p = 1f; var scaledTime = t / 1f - 1f; return scaledTime * scaledTime * ((p + 1f) * scaledTime + p) + 1f; }
    public static float BackInOut(float t)
    {
        var p = 1f;
        var scaledTime = t * 2f;
        var scaledTime2 = scaledTime - 2f;
        var s = p * 1.525f;

        if (scaledTime < 1f) return 0.5f * scaledTime * scaledTime * ((s + 1f) * scaledTime - s);
        else return 0.5f * (scaledTime2 * scaledTime2 * ((s + 1f) * scaledTime2 + s) + 2f);
    }

    public static float BounceIn(float t) { return 1f - BounceOut(1f - t); }
    public static float BounceOut(float t)
    {
        var scaledTime = t / 1f;

        if (scaledTime < 1 / 2.75f)
        {
            return 7.5625f * scaledTime * scaledTime;
        }
        else if (scaledTime < 2 / 2.75)
        {
            var scaledTime2 = scaledTime - 1.5f / 2.75f;
            return 7.5625f * scaledTime2 * scaledTime2 + 0.75f;
        }
        else if (scaledTime < 2.5 / 2.75)
        {
            var scaledTime2 = scaledTime - 2.25f / 2.75f;
            return 7.5625f * scaledTime2 * scaledTime2 + 0.9375f;
        }
        else
        {
            var scaledTime2 = scaledTime - 2.625f / 2.75f;
            return 7.5625f * scaledTime2 * scaledTime2 + 0.984375f;
        }
    }
    public static float BounceInOut(float t)
    {
        if (t < 0.5) return BounceIn(t * 2f) * 0.5f;
        else return BounceOut(t * 2f - 1f) * 0.5f + 0.5f;
    }
}
