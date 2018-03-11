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
using NP.Utilities.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.AttrTest
{
    public interface ISelectableData : IMyData, ISelectableItem<ISelectableData>
    {

    }

    public interface WrapperInterface
    {
        SelectableItem<ISelectableData> TheSelectableItem { get; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            ITypeConfig<WrapperInterface> typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableData, WrapperInterface>();

            typeConfig.ConfigurationCompleted();

            ISelectableData myInterfaceObj =
                Core.GetInstanceOfGeneratedType<ISelectableData>(typeConfig.ClassName) as ISelectableData;

            myInterfaceObj.FirstName = "Nick";
            myInterfaceObj.LastName = "Polyak";

            myInterfaceObj.IsSelectedChanged += MyInterfaceObj_IsSelectedChanged;

            myInterfaceObj.IsSelected = true;

            Core.Save("GeneratedCode");
        }

        private static void MyInterfaceObj_IsSelectedChanged(ISelectableItem<ISelectableData> obj)
        {
            Console.WriteLine("IsSelected changed");
        }
    }
}
