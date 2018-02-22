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
using NP.Utilities.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.StrongTypeEnumTest
{


    public class MyClass
    {
        public string Str { get; set; }

        public int I { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ITypeConfig<SingleWrapperInterface<ProductKind>> adapterTypeConfig =
                Core.FindOrCreateSingleWrapperTypeConfig<IProduct, ProductKind>();

            adapterTypeConfig.SetWrappedPropGetter<IProduct, ProductKind, string>
            (
                prod => prod.DisplayName,
                prodKind => prodKind.GetDisplayName()
            );

            adapterTypeConfig.SetWrappedPropGetter<IProduct, ProductKind, string>
            (
                prod => prod.Description,
                prodKind => prodKind.GetDescription()
            );

            adapterTypeConfig.ConfigurationCompleted();

            IProduct product = Core.GetInstanceOfGeneratedType<IProduct>(null, ProductKind.Information);

            Console.WriteLine($"{product.DisplayName}: {product.Description}");

            Core.Save("GeneratedCode");
        }
    }
}
