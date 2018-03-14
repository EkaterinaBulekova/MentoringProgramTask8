using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DALLayer.Entities
{
    public abstract class Entity
    {
        public abstract bool IsValid(ValidateType type);
        public abstract string DeleteRule { get; set; }

        public override bool Equals(object entity)
        {
            return entity != null
                   && entity is Entity
                   && this == (Entity)entity;

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("|");
            foreach (var property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                sb.Append($"{property.GetValue(this)}|");
            } 

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return GetType().GetProperties()
                .Where(e => Attribute.IsDefined(e, typeof(KeyAttribute))).GetHashCode();
        }

        public static bool operator ==(Entity entity1, Entity entity2)
        {
            if ((object)entity1 == null && (object)entity2 == null)
            {
                return true;
            }

            if ((object)entity1 == null || (object)entity2 == null)
            {
                return false;
            }

            if (entity1.GetType().GetProperties()
                .Where(e => Attribute.IsDefined(e, typeof(KeyAttribute)))
                .All(_=>_.GetValue(entity1).ToString() == _.GetValue(entity2).ToString()))
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(Entity entity1,
            Entity entity2)
        {
            return (!(entity1 == entity2));
        }
    }
}
