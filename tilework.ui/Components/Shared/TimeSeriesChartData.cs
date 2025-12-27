using System;
using System.Collections.Generic;

namespace Tilework.Ui.Components.Shared;

public class TimeSeriesChartData
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<DateTimeOffset, double> Data { get; set; } = new();
}
