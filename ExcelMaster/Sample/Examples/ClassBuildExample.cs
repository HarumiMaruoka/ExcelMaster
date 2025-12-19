using ExcelMaster.Builders;

namespace Sample.Examples
{
    internal class ClassBuildExample
    {
        public static void Run()
        {
            WorkbookGenerator.Generate("Assets/Excels/Item.xlsx", "Sample.csproj");
        }
    }
}
