﻿using FracturedJson;

namespace Tests;

/// <summary>
/// Tests about formatting things in tables, so that corresponding properties and array positions are neatly
/// lined up, when possible.
/// </summary>
[TestClass]
public class TableFormattingTests
{
    [TestMethod]
    public void NestedElementsLineUp()
    {
        var inputLines = new[]
        {
            "{",
            "    'Rect' : { 'position': {'x': -44, 'y':  3.4}, 'color': [0, 255, 255] }, ",
            "    'Point': { 'position': {'y': 22, 'z': 3} }, ",
            "    'Oval' : { 'position': {'x': 140, 'y':  0.04}, 'color': '#7f3e96' }  ",
            "}",
        };
        var input = string.Join("\n", inputLines).Replace('\'', '"');

        // With default options (except EOF), this will be neatly formatted as a table.
        var opts = new FracturedJsonOptions() { JsonEolStyle = EolStyle.Lf };

        var formatter = new Formatter() { Options = opts };
        var output = formatter.Reformat(input, 0);
        var outputLines = output.TrimEnd().Split('\n');

        // Everything should line up.
        TestHelpers.TestInstancesLineUp(outputLines, "x");
        TestHelpers.TestInstancesLineUp(outputLines, "y");
        TestHelpers.TestInstancesLineUp(outputLines, "z");
        TestHelpers.TestInstancesLineUp(outputLines, "position");
        TestHelpers.TestInstancesLineUp(outputLines, "color");

        // The numbers of the y column will be justified.
        StringAssert.Contains(outputLines[2], "22.00,");
    }


    [TestMethod]
    public void NestedElementsCompactWhenNeeded()
    {
        var inputLines = new[]
        {
            "{",
            "    'Rect' : { 'position': {'x': -44, 'y':  3.4}, 'color': [0, 255, 255] }, ",
            "    'Point': { 'position': {'y': 22, 'z': 3} }, ",
            "    'Oval' : { 'position': {'x': 140, 'y':  0.04}, 'color': '#7f3e96' }  ",
            "}",
        };
        var input = string.Join("\n", inputLines).Replace('\'', '"');

        // Smaller rows, so there's not enough room to do a full table.
        var opts = new FracturedJsonOptions() { MaxTotalLineLength = 77, JsonEolStyle = EolStyle.Lf };

        var formatter = new Formatter() { Options = opts };
        var output = formatter.Reformat(input, 0);
        var outputLines = output.TrimEnd().Split('\n');

        // Since the available size is reduced, x,y,z will no longer line up, but position and color will.
        TestHelpers.TestInstancesLineUp(outputLines, "position");
        TestHelpers.TestInstancesLineUp(outputLines, "color");

        // The numbers of the y column aren't justified, so the input value is used.
        StringAssert.Contains(outputLines[2], "22,");
    }


    [TestMethod]
    public void FallBackOnInlineIfNeeded()
    {
        var inputLines = new[]
        {
            "{",
            "    'Rect' : { 'position': {'x': -44, 'y':  3.4}, 'color': [0, 255, 255] }, ",
            "    'Point': { 'position': {'y': 22, 'z': 3} }, ",
            "    'Oval' : { 'position': {'x': 140, 'y':  0.04}, 'color': '#7f3e96' }  ",
            "}",
        };
        var input = string.Join("\n", inputLines).Replace('\'', '"');

        // In this case, it's too small to do any table formatting.  But each row should still be inlined.
        var opts = new FracturedJsonOptions() { MaxTotalLineLength = 74, JsonEolStyle = EolStyle.Lf };

        var formatter = new Formatter() { Options = opts };
        var output = formatter.Reformat(input, 0);
        var outputLines = output.TrimEnd().Split('\n');

        // All rows should be inlined, so a total of 5 rows.
        Assert.AreEqual(5, outputLines.Length);

        // Not even position lines up here.
        Assert.AreNotEqual(outputLines[1].IndexOf("position", StringComparison.Ordinal),
            outputLines[2].IndexOf("position", StringComparison.Ordinal));
    }


    [TestMethod]
    public void TablesWithCommentsLineUp()
    {
        var inputLines = new[]
        {
            "{",
            "'Firetruck': /* red */ { 'color': '#CC0000' }, ",
            "'Dumptruck': /* yellow */ { 'color': [255, 255, 0] }, ",
            "'Godzilla': /* green */  { 'color': '#336633' },  // Not a truck",
            "/* ! */ 'F150': { 'color': null } ",
            "}"
        };
        var input = string.Join("\n", inputLines).Replace('\'', '"');

        // Need to be wide enough and allow comments.
        var opts = new FracturedJsonOptions() { MaxTotalLineLength = 100, CommentPolicy = CommentPolicy.Preserve,
            JsonEolStyle = EolStyle.Lf };

        var formatter = new Formatter() { Options = opts };
        var output = formatter.Reformat(input, 0);
        var outputLines = output.TrimEnd().Split('\n');

        // All rows should be inlined, so a total of 5 rows.
        Assert.AreEqual(6, outputLines.Length);

        // Lots of stuff to line up here.
        TestHelpers.TestInstancesLineUp(outputLines, "\"");
        TestHelpers.TestInstancesLineUp(outputLines, ":");
        TestHelpers.TestInstancesLineUp(outputLines, " {");
        TestHelpers.TestInstancesLineUp(outputLines, " }");
        TestHelpers.TestInstancesLineUp(outputLines, "color");
    }

    [TestMethod]
    public void TablesWithBlankLinesLineUp()
    {
        var inputLines = new[]
        {
            "{'a': [7,8],",
            "",
            "//1",
            "'b': [9,10]}",
        };
        var input = string.Join("\n", inputLines).Replace('\'', '"');

        // Need to be wide enough and allow comments.
        var opts = new FracturedJsonOptions() { CommentPolicy = CommentPolicy.Preserve,
            PreserveBlankLines = true, JsonEolStyle = EolStyle.Lf };

        var formatter = new Formatter() { Options = opts };
        var output = formatter.Reformat(input, 0);
        var outputLines = output.TrimEnd().Split('\n');

        // All rows should be inlined, so a total of 5 rows.
        Assert.AreEqual(6, outputLines.Length);

        // The presence of comments and blank lines shouldn't prevent table formatting.
        TestHelpers.TestInstancesLineUp(outputLines, ":");
        TestHelpers.TestInstancesLineUp(outputLines, "[");
        TestHelpers.TestInstancesLineUp(outputLines, "]");
    }
}
