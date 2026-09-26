namespace MathExam.Core;

public enum Quantity
{
    Length,
    Mass,
    Capacity,
}

/// <summary>A metric unit: 10^<see cref="Exponent"/> of its quantity's base unit (m, g, L).</summary>
public sealed record MetricUnit(string Symbol, Quantity Quantity, int Exponent)
{
    public override string ToString() => Symbol;
}

/// <summary>The units of unit-conversion tasks, and the pairs that are asked.</summary>
public static class MetricUnits
{
    /// <summary>Units at most this many powers of ten apart are paired (×10, ×100 or ×1000).</summary>
    public const int MaxStep = 3;

    public static IReadOnlyList<MetricUnit> All { get; } =
    [
        new("mm", Quantity.Length, -3), new("cm", Quantity.Length, -2), new("dm", Quantity.Length, -1),
        new("m", Quantity.Length, 0), new("km", Quantity.Length, 3),
        new("mg", Quantity.Mass, -3), new("g", Quantity.Mass, 0), new("kg", Quantity.Mass, 3), new("t", Quantity.Mass, 6),
        new("mL", Quantity.Capacity, -3), new("cL", Quantity.Capacity, -2), new("dL", Quantity.Capacity, -1),
        new("L", Quantity.Capacity, 0), new("hL", Quantity.Capacity, 2),
    ];

    /// <summary>(larger, smaller) unit pairs of one quantity, at most <see cref="MaxStep"/> powers of ten apart.</summary>
    public static IReadOnlyList<(MetricUnit Large, MetricUnit Small)> Pairs { get; } =
        (from large in All
         from small in All
         where large.Quantity == small.Quantity && large.Exponent - small.Exponent is > 0 and <= MaxStep
         select (large, small)).ToList();

    /// <summary>How many of <paramref name="small"/> make one <paramref name="large"/>, e.g. 1000 for km and m.</summary>
    public static long Factor(MetricUnit large, MetricUnit small)
    {
        long factor = 1;
        for (var i = small.Exponent; i < large.Exponent; i++)
            factor *= 10;
        return factor;
    }
}
