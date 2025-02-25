using BlazorDatasheet.Data;
using BlazorDatasheet.Events;
using BlazorDatasheet.Formats;
using BlazorDatasheet.Render;

namespace BlazorDatasheet.Commands;

public class SetRangeFormatCommand : IUndoableCommand
{
    private readonly CellFormat _cellFormat;
    private readonly BRange _range;

    private IEnumerable<OrderedInterval<CellFormat>> _colFormats;
    private IEnumerable<OrderedInterval<CellFormat>> _rowFormats;
    private Dictionary<(int row, int col), CellChangedFormat> _cellFormatsChanged;

    /// <summary>
    /// Command to set the format of the range given. The cell format is merged into the existing format, so that
    /// only properties that are specifically defined in cellFormat are changed.
    /// </summary>
    /// <param name="cellFormat">The new cell format.</param>
    /// <param name="range">The range to set the format for. Can be a cell, column or row range.</param>
    public SetRangeFormatCommand(CellFormat cellFormat, BRange range)
    {
        _cellFormat = cellFormat;
        _range = range.Clone();
    }

    public bool Execute(Sheet sheet)
    {
        // Get the current states of row and column formats
        _colFormats = sheet.ColFormats.CloneAllIntervals();
        _rowFormats = sheet.RowFormats.CloneAllIntervals();
        // get the current format values of all cells
        var changed = sheet.SetFormatImpl(_cellFormat, _range);
        _cellFormatsChanged = new Dictionary<(int row, int col), CellChangedFormat>();

        foreach (var change in changed)
        {
            if (!_cellFormatsChanged.ContainsKey((change.Row, change.Col)))
                _cellFormatsChanged.Add((change.Row, change.Col),
                                        new CellChangedFormat(change.Row, change.Col, change.OldFormat?.Clone(),
                                                              _cellFormat));
        }

        return true;
    }

    public bool Undo(Sheet sheet)
    {
        sheet.RowFormats.Clear();
        sheet.ColFormats.Clear();

        sheet.RowFormats.AddRange(_rowFormats);
        sheet.ColFormats.AddRange(_colFormats);

        // build up format changed event args to emit
        var colRegionsChanged = _colFormats.Select(x => new ColumnRegion(x.Start, x.End));
        var rowRegionsChanged = _colFormats.Select(x => new RowRegion(x.Start, x.End));
        var cellsChangedBack = new List<CellChangedFormat>();

        foreach (var cellFormatChanged in _cellFormatsChanged.Values)
        {
            sheet.SetCellFormat(cellFormatChanged.Row, cellFormatChanged.Col, cellFormatChanged.OldFormat);
            cellsChangedBack.Add(new CellChangedFormat(cellFormatChanged.Row, cellFormatChanged.Col,
                                                       cellFormatChanged.OldFormat, cellFormatChanged.NewFormat));
        }

        _cellFormatsChanged.Clear();

        sheet.EmitFormatChanged(new FormatChangedEventArgs(cellsChangedBack, colRegionsChanged, rowRegionsChanged));
        return true;
    }
}