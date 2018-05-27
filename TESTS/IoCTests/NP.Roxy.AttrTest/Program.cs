﻿// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using NP.Concepts;
using NP.Concepts.Behaviors;
using System;

namespace NP.Roxy.AttrTest
{
    public interface ISelectableData : IMyData, ISelectableItem<ISelectableData>
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            //Core.SetWrapperType(typeof(ISelectableItem<>), typeof(ISelectableItemWrapper<>));
            Core.AddTypeAssemblyStatic<CodeBuilder>();

            ISelectableData myInterfaceObj =
                Core.CreateInstanceOfGeneratedType<ISelectableData>();

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
