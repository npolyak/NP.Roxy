using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy
{
    public interface ICompilationContainer
    {
        Compilation TheCompilation { get; }
    }

    public static class CompilationContainerExtensions
    {
        public static INamedTypeSymbol GetGenericTypeSymbol(this ICompilationContainer compilationContainer, Type type)
        {
            return type.GetGenericTypeSymbol(compilationContainer.TheCompilation);
        }

        public static INamedTypeSymbol 
            GetUnboundGenericTypeSymbol(this ICompilationContainer compilationContainer, Type type)
        {
            return type.GetUnboundGenericTypeSymbol(compilationContainer.TheCompilation);
        }

        public static INamedTypeSymbol GetTypeSymbol(this ICompilationContainer compilationContainer, Type type)
        {
            return type.GetTypeSymbol(compilationContainer.TheCompilation);
        }

        public static INamedTypeSymbol GetRealTypeSymbol<T>(this ICompilationContainer compilationContainer) =>
            RoslynAnalysisAndGenerationUtils.GetRealRoxyTypeSymbol<T>(compilationContainer.TheCompilation);

        public static INamedTypeSymbol GetNullForNoType
        (
            this ICompilationContainer compilationContainer, 
            INamedTypeSymbol typeSymbol
        )
        {
            return typeSymbol.GetNullForNoType(compilationContainer.TheCompilation);
        }
    }
}
