using System;

namespace DALLayer.Attributes
{
    public class StoredProcedureAttribute : Attribute
    {
        public string Name { get; set; }
        public string[] Params { get; set; }

        public StoredProcedureAttribute(string name, params string[] paramsName)
        {
            Name = name;
            Params = paramsName;
        }
    }
}