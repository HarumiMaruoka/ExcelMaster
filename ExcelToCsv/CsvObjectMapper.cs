using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ExcelToCsv
{
    public static class CsvObjectMapper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();

        /// <summary>
        /// 2次元配列のCSVデータ(行0:プロパティ名, 行1:型情報)からオブジェクト配列にマッピングします。
        /// </summary>
        /// <typeparam name="T">生成対象型</typeparam>
        /// <param name="csv">CSVセル値0,1行目はヘッダー</param>
        /// <returns>T の配列</returns>
        public static T[] MapCsvToObjects<T>(string[,] csv) where T : new()
        {
            if (csv == null) throw new ArgumentNullException(nameof(csv));
            var rows = csv.GetLength(0);
            var cols = csv.GetLength(1);
            if (rows < 3) return Array.Empty<T>();

            // ヘッダー取得
            var type = typeof(T);
            var propertyMap = _propertyCache.GetOrAdd(type, t =>
            {
                var dict = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.CanWrite)
                        dict[p.Name] = p;
                }
                return dict;
            });

            var columnInfos = new (PropertyInfo prop, Type propType)[cols];
            for (int c = 0; c < cols; c++)
            {
                var propName = csv[0, c];
                if (string.IsNullOrWhiteSpace(propName)) continue;
                if (propertyMap.TryGetValue(propName, out var pi))
                {
                    columnInfos[c] = (pi, pi.PropertyType);
                }
            }

            var result = new T[rows - 2]; // 上部2行はヘッダー行
            for (int r = 2; r < rows; r++)
            {
                var obj = new T();
                for (int c = 0; c < cols; c++)
                {
                    var info = columnInfos[c];
                    if (info.prop == null) continue; // 未対応列はスキップ
                    var raw = csv[r, c];
                    var value = ConvertCell(raw, info.propType);
                    info.prop.SetValue(obj, value);
                }
                result[r - 2] = obj;
            }
            return result;
        }

        /// <summary>
        /// セル文字列を指定型へ変換。空文字はデフォルト値/Nullableは null。
        /// </summary>
        private static object ConvertCell(string raw, Type targetType)
        {
            if (targetType == typeof(string)) return raw ?? string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                // Nullable<T> は null を返す
                if (IsNullable(targetType)) return null;
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (underlying.IsEnum)
                {
                    // 数値/名前 両対応
                    if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var enumInt))
                        return Enum.ToObject(underlying, enumInt);
                    return Enum.Parse(underlying, raw, ignoreCase: true);
                }
                if (underlying == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
                if (underlying == typeof(long)) return long.Parse(raw, CultureInfo.InvariantCulture);
                if (underlying == typeof(short)) return short.Parse(raw, CultureInfo.InvariantCulture);
                if (underlying == typeof(byte)) return byte.Parse(raw, CultureInfo.InvariantCulture);
                if (underlying == typeof(bool)) return ParseBool(raw);
                if (underlying == typeof(float)) return float.Parse(raw, CultureInfo.InvariantCulture);
                if (underlying == typeof(double)) return double.Parse(raw, CultureInfo.InvariantCulture);
                if (underlying == typeof(decimal)) return decimal.Parse(raw, CultureInfo.InvariantCulture);
                if (underlying == typeof(Guid)) return Guid.Parse(raw);
                if (underlying == typeof(DateTime)) return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                if (underlying == typeof(TimeSpan)) return TimeSpan.Parse(raw, CultureInfo.InvariantCulture);

                // Fallback
                return Convert.ChangeType(raw, underlying, CultureInfo.InvariantCulture);
            }
            catch
            {
                //変換失敗時はデフォルト値/Nullableは null を返す
                if (IsNullable(targetType)) return null;
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
        }

        private static bool IsNullable(Type t) => Nullable.GetUnderlyingType(t) != null;

        private static bool ParseBool(string raw)
        {
            raw = raw.Trim();
            if (bool.TryParse(raw, out var b)) return b;
            if (raw == "0") return false;
            if (raw == "1") return true;
            if (string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(raw, "no", StringComparison.OrdinalIgnoreCase)) return false;
            return false; // デフォルト false
        }
    }
}
