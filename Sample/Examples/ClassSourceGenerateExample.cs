namespace Sample.Samples
{
    internal class ClassSourceGenerateExample
    {
        public static void Run()
        {
            var classSource = ExcelMaster.MasterClassGenerator.Generate(
                usingNamespaces: new string[] { "System", "System.Collections.Generic" },
                classDefinition: new ExcelMaster.ClassDefinition
                {
                    Namespace = "MyGeneratedNamespace",
                    ClassName = "MyGeneratedClass",
                    Properties = new List<ExcelMaster.PropertyDefinition>
                    {
                        new ExcelMaster.PropertyDefinition
                        {
                            Name = "Id",
                            Type = "int",
                            Attributes = new string[] { "[Serializable]" }
                        },
                        new ExcelMaster.PropertyDefinition
                        {
                            Name = "Name",
                            Type = "string"
                        }
                    }
                }
            );

            Console.WriteLine(classSource);
        }
    }
}
