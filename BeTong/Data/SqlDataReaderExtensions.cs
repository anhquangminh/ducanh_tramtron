using System;
using System.Data;

namespace BeTong.Data
{
    internal static class SqlDataReaderExtensions
    {
        public static string GetStringOrEmpty(this IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? "" : Convert.ToString(value);
        }

        public static int GetIntOrDefault(this IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        public static double GetDoubleOrDefault(this IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? 0d : Convert.ToDouble(value);
        }

        public static DateTime? GetNullableDateTime(this IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
        }

        public static DateTime GetDateTimeOrDefault(this IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
        }
    }
}
