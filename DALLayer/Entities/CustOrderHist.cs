using System;
using DALLayer.Attributes;

namespace DALLayer.Entities
{
    [StoredProcedure("dbo.CustOrderHist", "CustomerID")]
    public class CustOrderHist : Entity
    {
        public string ProductName { get; set; }

        public int Total { get; set; }

        public override bool IsValid(ValidateType type)
        {
            throw new NotImplementedException();
        }

        public override string DeleteRule { get; set; }
    }
}
