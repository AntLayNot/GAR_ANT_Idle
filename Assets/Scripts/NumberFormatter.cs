using UnityEngine;

public static class NumberFormatter
{
    // K = mille, M = million, B = milliard, T = trillion, etc.
    private static readonly string[] suffixes = {
    "", "K", "M", "B", "T",
    "Qa", "Qi", "Sx", "Sp", "Oc", "No",
    "De", "Udc", "Ddc", "Tdc", "Qadc", "Qidc",
    "Sxdc", "Spdc", "Ocdc", "Nmdc",

    "Vg", "Uvg", "Dvg", "Tvg", "Qav", "Qvg",
    "Svg", "Spv", "Ovg", "Nvg", "Tg",

    // Extension
    "Qag", "Qig", "Sxg", "Spg", "Ocg", "Nng", "Ctg",
    "Uctg", "Dctg", "Tctg", "Qactg", "Qictg",
    "Sxctg", "Spctg", "Occtg", "Nnctg",

    "Mlg", "Umlg", "Dmlg", "Tmlg", "Qamg", "Qimg",
    "Sxmg", "Spmg", "Ocmg", "Nnmg",

    "Mmg", "Umrg", "Dmrg", "Tmrg", "Qarg", "Qirg",
    "Sxrg", "Sprg", "Ocrg", "Nnrg",

    // 100+ ordres
    "Gig", "Ugig", "Dg","Tg2","Qa2","Qi2","Sx2","Sp2","Oc2","No2",
    "De2","Ude2","Dde2","Tde2","Qade2","Qide2","Sxde2","Spde2","Ocde2","Node2"
};


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
