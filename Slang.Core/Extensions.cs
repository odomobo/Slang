using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    internal static class Extensions
    {
        //public static char? GetCharWithDefault(this string value, int index)
        //{
        //    if (index >= value.Length)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return value[index];
        //    }    
        //}

        public static IToken? GetOrDefault(this Span<IToken> self, int index)
        {
            if (index >= self.Length)
                return null;
            else
                return self[index];
        }
    }
}
