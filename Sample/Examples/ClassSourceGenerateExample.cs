namespace Sample.Samples
{
    internal class ClassSourceGenerateExample
    {
        public static void Run()
        {
            var classSource = CsvToSource.MasterClassGenerator.Generate(
                usingNamespaces: new string[] { "System", "System.Collections.Generic" },
                classDefinition: new CsvToSource.ClassDefinition
                {
                    Namespace = "MyGeneratedNamespace",
                    ClassName = "MyGeneratedClass",
                    Properties = new List<CsvToSource.PropertyDefinition>
                    {
                        new CsvToSource.PropertyDefinition
                        {
                            Name = "Id",
                            Type = "int",
                            Attributes = new string[] { "[Serializable]" }
                        },
                        new CsvToSource.PropertyDefinition
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
