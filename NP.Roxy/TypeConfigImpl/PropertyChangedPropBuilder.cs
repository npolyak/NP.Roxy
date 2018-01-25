using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeConfigImpl
{
    public class PropertyChangedPropBuilder : IMemberCodeBuilder<IPropertySymbol>
    {
        public static PropertyChangedPropBuilder ThePropertyChangedPropBuilder { get; } =
            new PropertyChangedPropBuilder();

        public void Build(IPropertySymbol symbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            string firePropertyChangedStr =
                $"this.OnPropertyChanged(nameof({symbol.Name}));";

            roslynCodeBuilder.AddPropWithBackingStore(symbol, null, firePropertyChangedStr);
        }
    }
}
