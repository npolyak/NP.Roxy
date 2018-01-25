using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeConfigImpl
{
    public class SimpleEventBuilder : IMemberCodeBuilder<IEventSymbol>
    {
        public virtual void Build(IEventSymbol symbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.AddEventDefinition(symbol);
        }
    }
}
