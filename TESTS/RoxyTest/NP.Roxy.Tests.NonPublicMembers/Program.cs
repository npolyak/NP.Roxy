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

namespace NP.Roxy.Tests.NonPublicMembers
{
    public interface WrapperInterface
    {
        MyData TheClass { get; }
    }

    // default and other implementations test
    // for events - easy
    // for get/set props - default impl Auto props
    // for get (or set) only abstract implementation
    // for methods - abstract impl

    // besides we have PropertyChanged implementation for get;set props
    // also lambda indirection implementations for props and methods
    // and throw not implemented exception for the methods.
    class Program
    {
        static void Main(string[] args)
        {
            #region WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
            ITypeConfig<IMyData, NoClass, WrapperInterface> typeConfig =
                Core.FindOrCreateTypeConfig<IMyData, NoClass, WrapperInterface>("MyType");

            typeConfig.SetPropMap
            (
                nameof(WrapperInterface.TheClass),
                "TheLastName",
                nameof(IMyData.LastName),
                true
            );

            typeConfig.SetPropMapAllowNonPublic(nameof(WrapperInterface.TheClass), nameof(IMyData.GetFullName));

            typeConfig.ConfigurationCompleted();

            IMyData myData = Core.GetInstanceOfGeneratedType<IMyData>("MyType");
            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            Console.WriteLine(myData.GetFullName());
            #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
        }
    }
}
