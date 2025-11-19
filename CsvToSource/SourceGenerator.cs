using Microsoft.CodeAnalysis;
using System;

namespace CsvToSource
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        // TODO: CSVまたはJSONで定義されたマスターデータ型情報のソースコードを自動生成する機能を実装する
        void ISourceGenerator.Execute(GeneratorExecutionContext context)
        {

        }

        void ISourceGenerator.Initialize(GeneratorInitializationContext context)
        {

        }
    }
}
