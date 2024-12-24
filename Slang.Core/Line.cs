using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public record Line (string Filename, string Text, int LineNumber)
    {
        public int Length => Text.Length;
        public char? this[int index]
        {
            get
            {
                if (index >= Text.Length)
                    return null;
                else
                    return Text[index];
            }
        }
    }
}
