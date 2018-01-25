// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using Microsoft.CodeAnalysis;
using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestProj;

namespace NP.Roxy.Demos.EnumToInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            Core.CreateEnumerationAdapter<IProduct, ProductKind>(typeof(ProductKindExtensions));

            IProduct product =
                Core.CreateEnumWrapper<IProduct, ProductKind>(ProductKind.FinancialInstrument);
                

            Console.WriteLine($"product: {product.GetDisplayName()}; Description: {product.GetDescription()}");

            Core.Save("GeneratedCode");
        }
    }
}
