using System;

namespace ExcelMaster
{
    /// <summary>
    /// Excel からバイナリをビルドするためのエントリポイントをマークする属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ExcelBinaryBuilderAttribute : Attribute
    {
        public string SheetName { get; }

        public ExcelBinaryBuilderAttribute(string sheetName)
        {
            SheetName = sheetName;
        }
    }
}