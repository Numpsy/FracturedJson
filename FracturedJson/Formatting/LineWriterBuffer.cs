﻿using System.IO;
using System.Text;

namespace FracturedJson.Formatting;

/// <summary>
/// An IBuffer for writing to a TextWriter (which will often be backed by a file or network stream).
/// Internally it composes each individual line before pushing those into writer.
/// </summary>
public class LineWriterBuffer : IBuffer
{
    /// <summary>
    /// Creates a new LineWriterBuffer.
    /// </summary>
    /// <param name="writer">TextWriter to which the sequence should be written.</param>
    /// <param name="trimTrailingWhitespace">If true, whitespace at the end of lines is removed.</param>
    public LineWriterBuffer(TextWriter writer, bool trimTrailingWhitespace)
    {
        _writer = writer;
        _trimTrailingWhitespace = trimTrailingWhitespace;
    }

    /// <summary>
    /// Add a single string to the buffer.
    /// </summary>
    public IBuffer Add(string value)
    {
        _lineBuff.Append(value);
        return this;
    }

    /// <summary>
    /// Add a group of strings to the buffer.
    /// </summary>
    public IBuffer Add(params string[] values)
    {
        foreach(var item in values)
            _lineBuff.Append(item);
        return this;
    }

    /// <summary>
    /// Call this only when sending an end-of-line symbol to the buffer.  Doing so helps the buffer with
    /// extra post-processing, like trimming trailing whitespace.
    /// </summary>
    public IBuffer EndLine(string eolString)
    {
        AddLineToWriter(eolString);
        return this;
    }

    /// <summary>
    /// Call this to let the buffer finish up any work in progress.
    /// </summary>
    public IBuffer Flush()
    {
        AddLineToWriter(string.Empty);
        _writer.Flush();
        return this;
    }

    private readonly TextWriter _writer;
    private readonly bool _trimTrailingWhitespace;
    private readonly StringBuilder _lineBuff = new();

    private void AddLineToWriter(string eolString)
    {
        if (_lineBuff.Length == 0 && eolString.Length == 0)
            return;

        var newLength = _lineBuff.Length;
        if (_trimTrailingWhitespace)
        {
            // Figure out where the end of the line's non-whitespace characters is.
            while (newLength > 0)
            {
                var ch = _lineBuff[newLength - 1];
                if (ch is not (' ' or '\t'))
                    break;
                newLength -= 1;
            }
        }

        // Write only up to the selected end to the Writer.
        _writer.Write(_lineBuff.ToString(0, newLength));
        _writer.Write(eolString);
        _lineBuff.Clear();
    }
}
