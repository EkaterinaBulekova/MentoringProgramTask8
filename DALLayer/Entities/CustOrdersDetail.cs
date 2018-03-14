using DALLayer.Attributes;
using System;

namespace DALLayer.Entities
{
    [StoredProcedure("dbo.CustOrdersDetail", "OrderID")]
    public class CustOrdersDetail : Entity
    {
        public string ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public short Quantity { get; set; }

        public double Discount { get; set; }

        public decimal ExtendedPrice { get; set; }


        public override bool IsValid(ValidateType type)
        {
            throw new NotImplementedException();
        }

        public override string DeleteRule { get; set; }
    }
}
