using ExcelMaster;
using ExcelMaster.Builders;

namespace Sample.Examples
{
    internal class ClassBuildExample
    {
        public static void Run()
        {
            var selection = ExcelUtil.ReadExcelToStringArray("Assets/Excels/Item.xlsx", "Sheet1");
            var @namespace = "GameNamespace";
            var className = "ItemData";
            var outputClassPath = "Assets/Generated/ItemData.cs";
            var outputDataPath = "Assets/Generated/ItemData_DataOnly.cs";
            var outputBuilderPath = "Assets/Generated/ItemData_BinaryBuilder.cs";
            var outputBinaryPath = "Assets/Generated/ItemData.bytes";

            // Class and enums (no Data)
            var source = SourceBuilder.GenerateClassSource(
                @namespace: @namespace,
                usingNamespaces: Array.Empty<string>(),
                className: className,
                selection: selection
            );

            // Data-only file as a nice partial class in the same namespace
            var dataSource = SourceBuilder.GenerateDataSection(
                @namespace: @namespace,
                usingNamespaces: Array.Empty<string>(),
                className: className,
                selection: selection
            );

            var builderSource = SourceBuilder.GenerateBinaryBuilder(
                @namespace: @namespace,
                usingNamespaces: new string[] { "Sample" },
                className: className,
                defaultOutputPath: outputBinaryPath
            );

            File.WriteAllText(outputClassPath, source);
            File.WriteAllText(outputDataPath, dataSource);
            File.WriteAllText(outputBuilderPath, builderSource);

            selection = ExcelUtil.ReadExcelToStringArray("Assets/Excels/Equipment.xlsx", "Sheet1");
            @namespace = "GameNamespace";
            className = "EquipmentData";
            outputClassPath = "Assets/Generated/EquipmentData.cs";
            outputDataPath = "Assets/Generated/EquipmentData_DataOnly.cs";
            outputBuilderPath = "Assets/Generated/EquipmentData_BinaryBuilder.cs";
            outputBinaryPath = "Assets/Generated/EquipmentData.bytes";

            // Class and enums (no Data)
            source = SourceBuilder.GenerateClassSource(
               @namespace: @namespace,
               usingNamespaces: Array.Empty<string>(),
               className: className,
               selection: selection
           );

            // Data-only file as a nice partial class in the same namespace
            dataSource = SourceBuilder.GenerateDataSection(
               @namespace: @namespace,
               usingNamespaces: Array.Empty<string>(),
               className: className,
               selection: selection
           );

            builderSource = SourceBuilder.GenerateBinaryBuilder(
               @namespace: @namespace,
               usingNamespaces: new string[] { "Sample" },
               className: className,
               defaultOutputPath: outputBinaryPath
           );

            File.WriteAllText(outputClassPath, source);
            File.WriteAllText(outputDataPath, dataSource);
            File.WriteAllText(outputBuilderPath, builderSource);

        }
    }
}
