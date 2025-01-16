namespace Geonorge.Forvaltning.Utils;

public class ColorGenerator
{
    private string[] _colors = [];

    public string[] Get() => _colors;

    public ColorGenerator Generate(string color, int count = 20)
    {
        var hsl = RgbToHsl(color);
        var hue = hsl[0] * 360;
        var step = 360 / count;
        var colors = new string[count];

        for (var i = 0; i < count; i++)
        {
            var newHue = (hue + i * step) % 360 / 360;
            var rgb = HslToRgb([newHue, hsl[1], hsl[2]]);

            colors[i] = rgb;
        }

        _colors = colors;

        return this;
    }

    public ColorGenerator Lighter(double value)
    {
        _colors = _colors
            .Select(color =>
            {
                var hsl = RgbToHsl(color);
                hsl[2] = Math.Min(hsl[2] + value, 1);

                return HslToRgb(hsl);
            })
            .ToArray();

        return this;
    }

    public ColorGenerator Darker(double value)
    {
        _colors = _colors
            .Select(color =>
            {
                var hsl = RgbToHsl(color);
                hsl[2] = Math.Max(hsl[2] - value, 0);

                return HslToRgb(hsl);
            })
            .ToArray();

        return this;
    }

    public ColorGenerator Purer(double value)
    {
        _colors = _colors
            .Select(color =>
            {
                var hsl = RgbToHsl(color);
                hsl[1] = Math.Min(hsl[1] + value, 1);

                return HslToRgb(hsl);
            })
            .ToArray();

        return this;
    }

    public ColorGenerator Impurer(double value)
    {
        _colors = _colors
            .Select(color =>
            {
                var hsl = RgbToHsl(color);
                hsl[1] = Math.Max(hsl[1] - value, 0);

                return HslToRgb(hsl);
            })
            .ToArray();

        return this;
    }

    private static string HslToRgb(double[] hsl)
    {
        var h = hsl[0];
        var s = hsl[1];
        var l = hsl[2];
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = HueToRgb(p, q, h + 1.0 / 3);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3);
        }

        double[] rgb = [r * 0xFF, g * 0xFF, b * 0xFF];
        var hex = rgb.Select(number => ToHex((int)Math.Round(number, MidpointRounding.AwayFromZero)));

        return $"#{string.Join("", hex)}";
    }

    private static double[] RgbToHsl(string color)
    {
        color = color[(color.IndexOf('#') + 1)..];

        if (color.Length == 3)
            color = $"{color[0]}{color[0]}{color[1]}{color[1]}{color[2]}{color[2]}";

        var r = (double)ParseHex(color[..2]) / 255;
        var g = (double)ParseHex(color.Substring(2, 2)) / 255;
        var b = (double)ParseHex(color.Substring(4, 2)) / 255;

        var max = new[] { r, g, b }.Max();
        var min = new[] { r, g, b }.Min();
        var diff = max - min;
        var l = (max + min) / 2;
        double h = 0;
        double s = 0;

        if (diff != 0)
        {
            s = l > 0.5 ? diff / (2 - max - min) : diff / (max + min);

            if (max == r)
                h = (g - b) / diff + (g < b ? 6 : 0);
            else if (max == g)
                h = (b - r) / diff + 2;
            else if (max == b)
                h = (r - g) / diff + 4;

            h /= 6;
        }

        return [h, s, l];
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0)
            t += 1;

        if (t > 1)
            t -= 1;

        if (t < 1.0 / 6)
            return p + (q - p) * 6 * t;

        if (t < 1.0 / 2)
            return q;

        if (t < 2.0 / 3)
            return p + (q - p) * (2.0 / 3 - t) * 6;

        return p;
    }

    private static int ParseHex(string hex) => int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

    private static string ToHex(int number) => number.ToString("X");
}
