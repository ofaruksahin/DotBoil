using DotBoil.Studio.Core.Attributes;
using MudBlazor;

namespace DotBoil.Studio.Core.Contracts;

public class ColumnSpanSize
{
    public int ColumnSpanXs { get; set; } = 12;
    
    public int ColumnSpanSm { get; set; } = 12;
    
    public int ColumnSpanMd { get; set; } = 12;
    
    public int ColumnSpanLg { get; set; } = 12;
    
    public int ColumnSpanXl { get; set; } = 12;

    public Breakpoint BreakPoint { get; set; } = Breakpoint.None;
}