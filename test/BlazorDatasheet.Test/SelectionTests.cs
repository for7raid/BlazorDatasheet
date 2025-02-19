﻿using System.Linq;
using BlazorDatasheet.Data;
using BlazorDatasheet.Selecting;
using NUnit.Framework;

namespace BlazorDatasheet.Test;

public class SelectionManagerTests
{
    private Sheet _sheet;

    [SetUp]
    public void Setup()
    {
        _sheet = new Sheet(3, 3);
    }

    [Test]
    public void Add_Selection_As_Entire_Sheet_Selects_All_cells_And_Sets_Active()
    {
        var selection = new Selection(_sheet);
        //Select all
        selection.SetSingle(_sheet.Region);
        var cells = selection.GetCells().ToList();
        Assert.AreEqual(_sheet.Region.Area, cells.Count);
    }

    public void Add_Selection_Changes_Active_Selection()
    {
        var selection = new Selection(_sheet);
        var r = new Region(0, 0);
        selection.SetSingle(r);
        Assert.AreEqual(r, selection.ActiveRegion);
        Assert.AreEqual(r.TopLeft, selection.ActiveCellPosition);
        var r2 = new Region(1, 1);
        Assert.AreEqual(r2, selection.ActiveRegion);
        Assert.AreEqual(r2.TopLeft, selection.ActiveCellPosition);
    }

    public void Add_Selection_Outside_Region_Constrains_Inside()
    {
        var selection = new Selection(_sheet);
        selection.SetSingle(new Region(-1000, 1000, -1000, 1000));
        Assert.AreEqual(_sheet.Region.Area, selection.ActiveRegion.GetIntersection(_sheet.Region).Area);
    }

    [Test]
    public void Cycle_Active_position_through_range()
    {
        var selection = new Selection(_sheet);
        selection.SetSingle(new Region(0, 1, 0, 1));


        Assert.AreEqual(new CellPosition(0, 0), selection.ActiveCellPosition);
        selection.MoveActivePositionByRow(1);
        Assert.AreEqual(new CellPosition(1, 0), selection.ActiveCellPosition);
        selection.MoveActivePositionByRow(1);
        Assert.AreEqual(new CellPosition(0, 1), selection.ActiveCellPosition);
        selection.MoveActivePositionByRow(1);
        Assert.AreEqual(new CellPosition(1, 1), selection.ActiveCellPosition);
        selection.MoveActivePositionByRow(1);
        Assert.AreEqual(new CellPosition(0, 0), selection.ActiveCellPosition);
    }

    [Test]
    public void Cycle_Active_position_through_ranges()
    {
        var selection = new Selection(_sheet);
        var r1 = new Region(0, 0, 0, 0);
        var r2 = new Region(1, 1, 1, 1);

        selection.SetSingle(0, 0);
        selection.BeginSelectingCell(1, 1);
        selection.EndSelecting();

        Assert.AreEqual(r2, selection.ActiveRegion);
        Assert.AreEqual(r2.TopLeft, selection.ActiveCellPosition);

        selection.MoveActivePositionByRow(1);

        Assert.AreEqual(r1, selection.ActiveRegion);
        Assert.AreEqual(r1.TopLeft, selection.ActiveCellPosition);
    }

    [Test]
    public void Selection_Event_Changed_Fires_Correctly()
    {
        var nTimesChanged = 0;
        var selection = new Selection(_sheet);
        selection.SelectionChanged += (sender, ranges) => { nTimesChanged++; };

        selection.SetSingle(new Region(1, 1));
        selection.SetSingle(new Region(2, 2));
        selection.ClearSelections();
        Assert.AreEqual(3, nTimesChanged);
    }

    [Test]
    public void Moves_Input_Position_When_Selecting_Region_Is_One_Cell()
    {
        _sheet.Selection.SetSingle(0, 0);
        _sheet.Selection.MoveActivePositionByRow(1);
        Assert.AreEqual(new CellPosition(1, 0), _sheet.Selection.GetInputPosition());
        Assert.AreEqual(new CellPosition(1, 0), _sheet.Selection.ActiveCellPosition);
    }

    [Test]
    public void Active_Selection_Moves_Over_Merged_cells()
    {
        _sheet.MergeCells(new Region(0, 1, 0, 1));
        _sheet.Selection.SetSingle(0, 0);
        Assert.AreEqual(4, _sheet.Selection.ActiveRegion.Area);
        _sheet.Selection.MoveActivePositionByRow(1);
        Assert.AreEqual(new CellPosition(2, 0), _sheet.Selection.ActiveCellPosition);
    }
}