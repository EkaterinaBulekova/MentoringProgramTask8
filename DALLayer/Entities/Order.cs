using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DALLayer.Entities
{
    [Table("Orders")]
    public class Order : Entity
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        public string CustomerID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        public DateTime? OrderDate { get; set; }

        [Required]
        public DateTime? RequiredDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        [Required]
        public int ShipVia { get; set; }

        [Required]
        public decimal Freight { get; set; }

        [Required]
        public string ShipName { get; set; }

        [Required]
        public string ShipAddress { get; set; }

        [Required]
        public string ShipCity { get; set; }

        [Required]
        public string ShipRegion { get; set; }

        [Required]
        public string ShipPostalCode { get; set; }

        [Required]
        public string ShipCountry { get; set; }

        public StatusType Status
        {
            get
            {
                var stat = StatusType.New;
                if (OrderDate != null) stat = StatusType.InWork;
                if (ShippedDate != null) stat = StatusType.Completed;
                return stat;
            }
        }

        public virtual List<OrderDetail> Details { get; set; }

        public override bool IsValid(ValidateType type)
        {
            switch (type)
            {
                case ValidateType.ForDelete:
                    if(Status != StatusType.Completed) 
                        return true;
                    return false;
                case ValidateType.ForInsert:
                    return true;
                case ValidateType.ForUpdate:
                    if(Status == StatusType.New)return true;
                        return false;
                default:
                    return false;
            }
        }

        public override string DeleteRule { get; set; } = " AND ShippedDate is NULL ";
    }
}
