using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using DALLayer.Entities;

namespace DALLayer.Extentions
{
    public static class ReaderMapers
    {
        public static T Map<T, T1>(this DbDataReader reader) where T : Entity where T1 : Entity
        {
            if (!reader.HasRows) return null;
            T item = null;
            if(reader.Read())
                item = Map<T>(reader);
            var listProperty = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(_ => _.PropertyType.IsAssignableFrom(typeof(List<T1>)));
            if (listProperty != null && reader.NextResult())
                listProperty.SetValue(item, reader.Maps<T1>());
            return item;
        }

        public static List<T> Maps<T>(this DbDataReader reader) where T : Entity
        {
            var items = new List<T>();
            while (reader.Read())
            {
                items.Add(reader.Map<T>());
            }
            return items;
        }

        private static T Map<T>(this IDataRecord reader) where T : Entity
        {
            var item = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < reader.FieldCount; i++)
                {
                    var prop = props.FirstOrDefault(_ => reader.GetName(i) == _.Name && !reader.IsDBNull(reader.GetOrdinal(_.Name)));
                    if (prop != null) prop.SetValue(item, reader[prop.Name]);
                }

            return item;
        }
    }
}
