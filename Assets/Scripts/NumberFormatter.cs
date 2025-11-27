using UnityEngine;

public static class NumberFormatter
{
    // K = mille, M = million, B = milliard, T = trillion, etc.
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "De", "Udc", "Ddc", "Tdc",
    "Qadc", "Qidc", "Sxdc", "Spdc", "Ocdc", "Nmdc", "Vg", "Uvg", "Dvg", "Tvg", "Qav", "Qvg", "Svg", "Spv", "Ovg", "Nvg", "Tg"};

    public static string Format(double value)
    {
        if (value < 1000d)
            return value.ToString("0.##");

        int suffixIndex = 0;
        while (value >= 1000d && suffixIndex < suffixes.Length - 1)
        {
            value /= 1000d;
            suffixIndex++;
        }

        // Utilise la culture de l’OS (donc virgule en FR)
        return value.ToString("0.##") + suffixes[suffixIndex];
    }
}
