using System;

namespace DALLayer.Attributes
{
    public class JoinedAttribute : Attribute
    {
        public string Table { get; set; }
        public string Key { get; set; }

        public JoinedAttribute(string table, string key)
        {
            Table = table;
            Key = key;
        }
    }
}