using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public record CharHandle
    {
        public string File { get; init; }
        public Line[] Lines { get; init; }
        public Line CurrentLine => Lines[_line];
        private int _line { get; init; }
        private int _linePos { get; init; }
        private int _filePos { get; init; }
        public int CurrentLineNumber => _line + 1;
        public int CurrentLinePosition => _linePos + 1;

        public CharHandle(string file, IEnumerable<Line> lines)
        {
            File = file;
            Lines = lines.ToArray();

            _line = 0;
            _linePos = 0;
            _filePos = 0;
        }

        public char? this[int offset]
        {
            get
            {
                // we could allow this, but let's not to keep the parser simpler
                if (offset < 0)
                    throw new InvalidOperationException();

                var index = offset + _filePos;

                if (index >= File.Length)
                    return null;
                else
                    return File[index];
            }
        }

        [Pure]
        public CharHandle Advance(int offset = 1)
        {
            int line = _line;
            int linePos = _linePos;
            int filePos = _filePos;
            for (int i = 0; i < offset; i++)
            {
                if (filePos >= File.Length)
                    break;

                filePos++;
                linePos++;

                // don't move to the next line if there isn't a next line
                if (linePos >= CurrentLine.Length && _line < Lines.Length)
                {
                    line++;
                    linePos = 0;
                }
            }

            return this with { 
                _line = line,
                _linePos = linePos,
                _filePos = filePos,
            };
        }

        [Pure]
        public bool EndOfStream()
        {
            return _filePos >= File.Length;
        }

        [Pure]
        public Location Location(CharHandle? endLocation = null)
        {
            int length = 1;
            if (endLocation != null)
            {
                if (endLocation._filePos < _filePos)
                    throw new InvalidOperationException();

                // if the lines are different, then we'll just say it's all the rest of this line
                if (endLocation._line != _line)
                {
                    length = CurrentLine.Length - _linePos;
                }
                else
                {
                    length = endLocation._linePos - _linePos;
                }
            }

            return new Location(CurrentLine, _linePos, length);
        }

        [Pure]
        public string GetString(CharHandle endLocation)
        {
            if (endLocation._filePos < _filePos)
                throw new InvalidOperationException();

            var length = endLocation._filePos - _filePos;
            return File.Substring(_filePos, length);
        }

        [Pure]
        public Error Error(string message = "Unknown error", CharHandle? endLocation = null)
        {
            var location = Location(endLocation);
            return new Error(location, message);
        }
    }
}
