using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceImplTest
{
    public interface MyInterfaceToTestCircularReference
    {
        int TheInt { get; set; }

        string TheStr { get; set; }
    }
}
