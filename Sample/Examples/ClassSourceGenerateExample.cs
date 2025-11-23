using ExcelMaster;

namespace Sample.Examples
{
    internal class ClassSourceGenerateExample
    {
        public static void Run()
        {
            var classSource = MasterClassGenerator.Generate(
                usingNamespaces: new string[] { "System", "System.Collections.Generic" },
                classDefinition: new ClassDefinition
                {
                    Namespace = "MyGeneratedNamespace",
                    ClassName = "MyGeneratedClass",
                    Properties = new List<PropertyDefinition>
                    {
                        new PropertyDefinition
                        {
                            Name = "Id",
                            Type = "int",
                            Attributes = new string[] { "[Serializable]" }
                        },
                        new PropertyDefinition
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
