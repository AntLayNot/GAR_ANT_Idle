using System.Text;

public static class NumberFormatter
{
    public static string Format(double value)
    {
        // Sécurité : si la valeur est invalide → on affiche 0
        if (double.IsNaN(value))
            return "0";

        // Si c'est infini, on affiche un symbole
        if (double.IsInfinity(value))
            return "∞";

        if (value < 1000d)
            return value.ToString("0.##");

        int tier = 0;

        while (value >= 1000d)
        {
            value /= 1000d;
            tier++;

            // Petit garde-fou (même si en pratique tier <= ~100)
            if (tier > 1000)
                break;
        }

        string suffix = TierToAlphabet(tier);
        return value.ToString("0.##") + suffix;
    }

    private static string TierToAlphabet(int tier)
    {
        if (tier <= 0) return "";

        tier--; // base 0
        StringBuilder sb = new StringBuilder();

        while (tier >= 0)
        {
            int letterIndex = tier % 26;
            char c = (char)('a' + letterIndex);
            sb.Insert(0, c);

            tier = (tier / 26) - 1;
        }

        return sb.ToString();
    }
}
