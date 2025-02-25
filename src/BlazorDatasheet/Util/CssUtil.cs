using System.Text;
using BlazorDatasheet.Data;
using BlazorDatasheet.Interfaces;
using BlazorDatasheet.Render;

namespace BlazorDatasheet.Util;

public class CssUtil
{
    /// <summary>
    /// Returns correctly styled input background colour & foreground colour, given the cell's formatting.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static string GetStyledInput(IReadOnlyCell cell)
    {
        var str = new StringBuilder();
        if (cell == null || cell.Formatting == null)
        {
            str.Append("background:var(--sheet-bg-color);");
            str.Append("color:var(--sheet-foreground-color)");
        }
        else if (cell.Formatting != null)
        {
            str.Append($"background:{cell.Formatting.BackgroundColor};");
            str.Append($"color:{cell.Formatting.ForegroundColor};");
        }
        return str.ToString();
    }

    /// <summary>
    /// Returns the css strings for producing width & max width of a cell given its location and spon
    /// </summary>
    /// <param name="col"></param>
    /// <param name="colSpan"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static string GetCellWidthStyles(int col, int colSpan, CellLayoutProvider provider)
    {
        var width = provider.ComputeWidth(col, colSpan);
        var str = new StringBuilder();
        str.Append($"width:{width}px;");
        str.Append($"max-width:{width}px;");
        str.Append($"min-width:{width}px;");
        return str.ToString();
    }
}