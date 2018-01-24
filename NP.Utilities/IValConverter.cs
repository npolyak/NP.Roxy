// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author(s) of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NP.Utilities
{
    public interface IValConverter<InputType, OutputType>
    {
        OutputType Convert(InputType sourceObj);
    }

    public interface IValConverter : IValConverter<object, object>
    {
    }

    public static class ValConverterExtensions
    {
        public static Func<InputType, OutputType>
            ToDelegate<InputType, OutputType>
            (
                this IValConverter<InputType, OutputType> converter
            )
        {
            if (converter == null)
                return null;

            return converter.Convert;
        }
    }
}
