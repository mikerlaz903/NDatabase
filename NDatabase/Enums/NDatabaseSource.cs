using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDatabase.Enums
{
    public enum NDatabaseSource : short
    {
        Default = 0,
        FromServer = 1,
        FromMemory = 2,
        FromCombined = 3
    }
}
