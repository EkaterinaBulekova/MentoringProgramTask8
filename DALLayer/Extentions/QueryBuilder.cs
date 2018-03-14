using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using DALLayer.Attributes;
using DALLayer.Entities;
using DALLayer.Recources;

namespace DALLayer.Extentions
{
    public static class QueryBuilder
    {
        private const string Select = "SELECT * FROM ";
        private const string Insert = "INSERT INTO";
        private const string Delete = "DELETE FROM {0} WHERE {1}={2} {3} ";
        private const string Values = " VALUES ";
        private const string Update = "UPDATE {0} SET {1} WHERE {2}={3}";
        private const string SelectWhere = "SELECT * FROM {0} {1} WHERE {2}={3} ";
        private const string Join = " AS a JOIN [{0}] AS b ON a.{1} = b.{2}";

        #region StoredPocedure
        public static DbCommand GetStoredProcCommand<T>(this DbCommand cmd, params object[] paramObjects)
        {
            var procedure = GetProcedureName<T>();
            if (string.IsNullOrEmpty(procedure))
                throw new Exception(ExceptionMessage.NoStoredProcedureAttributeWasFound);
            var paramNames = GetProcedureParamsName<T>().ToList();
            if(paramObjects.Length != paramNames.Count)
                throw new Exception(ExceptionMessage.WrongCountOfStoredProcedureParameters);
            var query = $"{GetProcedureName<T>()}";
            for (var i = 0; i < paramObjects.Length; i++)
            {
                var param = cmd.CreateParameter();
                param.ParameterName = paramNames[i];
                param.Value = paramObjects[i];
                cmd.Parameters.Add(param);
            }

            cmd.CommandText = query;
            return cmd;
        }
        private static string GetProcedureName<T>()
        {
            var procAttr = Attribute.GetCustomAttribute(typeof(T),
                typeof(StoredProcedureAttribute));
            return procAttr != null
                ? $"{(procAttr as StoredProcedureAttribute)?.Name}"
                : string.Empty;
        }

        private static IEnumerable<string> GetProcedureParamsName<T>()
        {
            var procAttr = Attribute.GetCustomAttribute(typeof(T),
                typeof(StoredProcedureAttribute));
            return (procAttr as StoredProcedureAttribute)?.Params.Select(_=>"@"+_);
        }

        #endregion

        #region Insert
        public static DbCommand GetInsertCommand<T>(this DbCommand cmd, T item)
        {
            var table = GetTableName<T>();
            if (string.IsNullOrEmpty(table))
                throw new Exception(ExceptionMessage.NoTableAttributeWasFound);
            var query = $"{Insert} {table} {GetInsertFieldList(item)}";
            cmd.CommandText = query;
            return cmd;
        }

        private static string GetInsertFieldList<T>(T item)
        {
            var prb = new StringBuilder();
            var valb = new StringBuilder();

            var properties = GetRequiredFields(item);
            prb.Append(" (");
            valb.Append($"{Values}(");
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetValue(item) == null) continue;
                var property = GetSqlValue(item, propertyInfo);
                valb.Append($"{GetFormattedInsertField(propertyInfo, property)},");
                prb.Append($"{propertyInfo.Name},");
            }

            valb.Replace(",", ") ", valb.Length - 1, 1);
            prb.Replace(",", ") ", prb.Length - 1, 1);
            var query = prb.ToString() + valb;
            return query;
        }

        private static string GetFormattedInsertField(PropertyInfo propertyInfo, SqlString property)
        {
            // int
            var result = $"{property.Value}";
            // string
            if (propertyInfo.PropertyType == typeof(string))
            {
                result = $"'{property.Value}'";
            }
            // datetime
            else if (propertyInfo.PropertyType == typeof(DateTime))
            {   
                result = $"'{property.Value:u}'";
            }
            return result;
        }
        #endregion

        #region Update
        public static DbCommand GetUpdateCommand<T>(this DbCommand cmd, T item)
        {
            var table = GetTableName<T>();
            if (string.IsNullOrEmpty(table))
                throw new Exception(ExceptionMessage.NoTableAttributeWasFound);
            var query = string.Format(Update,table,GetUpdateFieldList(item),GetKeyFieldName(item),GetKeyFieldValue(item));
            cmd.CommandText = query;
            return cmd;
        }

        private static string GetUpdateFieldList<T>(T item)
        {
            var sb = new StringBuilder();
            var properties = GetRequiredFields(item);
            var keyField = GetKeyFieldName(item);
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetValue(item) == null) continue;
                if (keyField == propertyInfo.Name) continue;
                var property = GetSqlValue(item, propertyInfo);
                sb.Append(GetFormattedUpdateField(propertyInfo, property));
            }
            var query = sb.ToString();
            return query.Remove(query.Length - 1);
        }

        private static string GetFormattedUpdateField(PropertyInfo propertyInfo, SqlString property)
        {
            // int
            var result = $"{propertyInfo.Name}={property.Value},";
            // string
            if (propertyInfo.PropertyType == typeof(string))
            {
                result = $"{propertyInfo.Name}='{property.Value}',";
            }
            // datetime
            else if (propertyInfo.PropertyType == typeof(DateTime))
            {
                result = $"{propertyInfo.Name}='{property.Value:u}',";
            }
            return result;
        }
        #endregion

        #region Delete
        public static void GetDeleteCommand<T,T1>(this DbCommand cmd, object id) where T : Entity, new () where T1 : Entity, new()
        {
            var table = GetTableName<T>();
            if (string.IsNullOrEmpty(table))
                throw new Exception(ExceptionMessage.NoTableAttributeWasFound);
            var subtable = GetTableName<T1>();
            if (string.IsNullOrEmpty(subtable))
                throw new Exception(ExceptionMessage.NoTableAttributeWasFound);
            var item = new T();
            var subitem = new T1();
            var query = string.Format(Delete, subtable, GetForeinKeyField(subitem), id, subitem.DeleteRule);
                query += string.Format(Delete, table, GetKeyFieldName(item), id, item.DeleteRule);
            cmd.CommandText = query;
        }
        public static void GetDeleteCommand<T>(this DbCommand cmd, object id) where T : Entity
        {
            var table = GetTableName<T>();
            var item = Activator.CreateInstance<T>();
            if (string.IsNullOrEmpty(table))
                throw new Exception(ExceptionMessage.NoTableAttributeWasFound);
            var query = string.Format(Delete, table, GetKeyFieldName(item), id, item.DeleteRule);
            cmd.CommandText = query;
        }

        #endregion

        #region Select
        public static void GetSelectByIdCommand<T, T1>(this DbCommand cmd, object id) where T : new() where T1 : new()
        {
            var table = GetTableName<T>();
            var subtable = GetTableName<T1>();
            if (string.IsNullOrEmpty(table))
                throw new Exception(ExceptionMessage.NoTableAttributeWasFound);
            var query = string.Format(SelectWhere, table, GetJoinString<T>(), GetKeyFieldName(new T()), id);
            if (string.IsNullOrEmpty(subtable))
                throw new Exception(ExceptionMessage.NoTableAttributeWasFound);
            query += string.Format(SelectWhere, subtable, GetJoinString<T1>(), GetForeinKeyField(new T1()), id);
            cmd.CommandText = query;
        }

        public static void GetSelectAllCommand<T>(this DbCommand cmd)
        {
            var table = GetTableName<T>();
            if (string.IsNullOrEmpty(table))
                throw new Exception(ExceptionMessage.NoTableAttributeWasFound);
            var query = $"{Select} {table}";
            cmd.CommandText = query;
        }

        #endregion

        #region Helper methods        

        private static string GetTableName<T>()
        {
            var tableAttr = Attribute.GetCustomAttribute(typeof(T),
                typeof(TableAttribute));
            return tableAttr != null
                ? $"[{(tableAttr as TableAttribute)?.Name}]"
                : string.Empty;
        }

        private static string GetJoinString<T>()
        {
            var joinedProperty = typeof(T).GetProperties()
                .FirstOrDefault(e => e.GetCustomAttribute(typeof(JoinedAttribute)) != null);
            JoinedAttribute joinedAttr = null;
            if (joinedProperty != null)
                joinedAttr = joinedProperty.GetCustomAttribute(typeof(JoinedAttribute)) as JoinedAttribute;
            return joinedAttr != null ? string.Format(Join, joinedAttr.Table, joinedAttr.Key, joinedAttr.Key) 
                : string.Empty;
        }


        private static SqlString GetSqlValue<T>(T item, PropertyInfo propertyInfo)
        {
            return new SqlString(propertyInfo.GetValue(item).ToString());
        }

        private static string GetKeyFieldName<T>(T item)
        {
            var result = GetKeyField(item);
            return result.Name;
        }

        private static string GetKeyFieldValue<T>(T item)
        {
            var result = GetKeyField(item);
            return result.GetValue(item).ToString();
        }

        private static PropertyInfo GetKeyField<T>(T item)
        {
            var keyField = item
                .GetType()
                .GetProperties()
                .FirstOrDefault(e => Attribute.IsDefined(e, typeof(KeyAttribute)));
            if (keyField != null)
            {
                return keyField;
            }
            throw new Exception(ExceptionMessage.KeyOnPropertyCouldNotFound);
        }

        private static IEnumerable<PropertyInfo> GetRequiredFields<T>(T item)
        {
            var requredFields = item
                .GetType()
                .GetProperties()
                .Where(e => Attribute.IsDefined(e, typeof(RequiredAttribute))).ToList();
            if (requredFields.Any())
            {
                return requredFields;
            }
            throw new Exception(ExceptionMessage.RequiredPropertyCouldNotFound);
        }

        private static string GetForeinKeyField<T>(T item)
        {
            var keyField = item
                .GetType()
                .GetProperties()
                .FirstOrDefault(e => Attribute.IsDefined(e, typeof(ForeignKeyAttribute)));
            if (keyField != null)
            {
                return keyField.Name;
            }
            throw new Exception(ExceptionMessage.ForeignKeyOnPropertyCouldNotFound);
        }
        #endregion
    }
}
