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

using NP.Roxy.TypeConfigImpl;
using System;

namespace NP.Roxy.Demos.PropChangedInterfaceImpl
{
    class Program
    {
        static void Main(string[] args)
        {
            ITypeConfig typeConfig = Core.FindOrCreateTypeConfigByTypeToImpl<IPerson>("Person");

            typeConfig.SetEventBuilder(PropertyChangedEventBuilder.ThePropertyChangedEventBuilder, "PropertyChanged");

            typeConfig.SetPropBuilder
            (
                PropertyChangedPropBuilder.ThePropertyChangedPropBuilder,
                nameof(IPerson.Age),
                nameof(IPerson.Profession)
            );

            typeConfig.ConfigurationCompleted();

            IPerson person = Core.GetInstanceOfGeneratedType<IPerson>("Person");

            person.FirstName = "Joe";

            person.LastName = "Doe";

            person.Age = 35;

            person.Profession = "Astronaut";

            Console.WriteLine($"Name='{person.FirstName} {person.LastName}'; Age='{person.Age}'; Profession='{person.Profession}'");

            Core.Save("GeneratedCode");
        }
    }
}
