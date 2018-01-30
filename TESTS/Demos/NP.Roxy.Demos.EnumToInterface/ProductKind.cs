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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Demos.EnumToInterface
{
    public enum ProductKind
    {
        Grocery,
        FinancialInstrument,
        Information
    }

    public static class ProductKindExtensions
    {
        // returns a displayable short name for the ProductKind
        public static string GetDisplayName(this ProductKind productKind)
        {
            switch(productKind)
            {
                case ProductKind.Grocery:
                    return "Grocery";
                case ProductKind.FinancialInstrument:
                    return "Financial Instrument";
                case ProductKind.Information:
                    return "Information";
            }

            return null;
        }

        // returns the full description of the ProductKind
        // note that the method is private
        private static string GetDescription(this ProductKind productKind)
        {
            switch (productKind)
            {
                case ProductKind.Grocery:
                    return "Products you can buy in a grocery store";
                case ProductKind.FinancialInstrument:
                    return "Products you can buy on a stock exchange";
                case ProductKind.Information:
                    return "Products you can get on the Internet";
            }

            return null;
        }
    }
}