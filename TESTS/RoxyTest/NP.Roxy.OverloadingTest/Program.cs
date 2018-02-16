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

namespace NP.Roxy.OverloadingTest
{
    public interface IMyDataImplWrapper
    {
        MyDataImplementorClass DataImplementor { get; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            #region TypeSave prop map setting

            //ITypeConfig<IMyDataImplWrapper> typeConfig =
            //    Core.FindOrCreateTypeConfig<IMyData, IMyDataImplWrapper>();

            //typeConfig.SetPropMemberMap<IMyData, MyDataImplementorClass, string>
            //(
            //    (wrapper) => wrapper.DataImplementor,
            //    (dataImpl) => dataImpl.FullName1,
            //    (data) => data.FullName
            //);

            //typeConfig.ConfigurationCompleted();

            //IMyData myData =
            //    Core.GetInstanceOfGeneratedType<IMyData>();

            //myData.FirstName = "Joe";
            //myData.LastName = "Doe";

            //Console.WriteLine(myData.FullName);

            #endregion TypeSave prop map setting

            #region Prop Getter 
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IMyData, NoInterface>();

            typeConfig.SetPropGetter<IMyData, string>
            (
                (data) => data.FullName,
                (data) => data.FirstName + " " + data.LastName
            );

            typeConfig.ConfigurationCompleted();

            #endregion Prop Getter 

            Core.Save("GeneratedCode");
        }
    }
}
