using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeConfigImpl
{
    public class PropertyChangedEventBuilder : SimpleEventBuilder
    {
        public static PropertyChangedEventBuilder ThePropertyChangedEventBuilder { get; } =
            new PropertyChangedEventBuilder();

        public override void Build(IEventSymbol symbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            base.Build(symbol, roslynCodeBuilder);

            roslynCodeBuilder.AddEmptyLine();

            roslynCodeBuilder.AddLine("protected void OnPropertyChanged(string propName)");
            roslynCodeBuilder.Push();
            roslynCodeBuilder.AddLine($"this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName))", true);
            roslynCodeBuilder.Pop();
        }
    }
}
