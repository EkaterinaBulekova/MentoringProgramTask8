using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DALLayer.Attributes;

namespace DALLayer.Entities
{
    [Table("Order Details")]
    public class OrderDetail : Entity
    {
        [Key]
        [ForeignKey("OrderID")]
        [Required]
        public int OrderID { get; set; }

        [Key]
        [Required]
        public int ProductID { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public short Quantity { get; set; }

        [Required]
        public double Discount { get; set; }

        [Joined("Products", "ProductID")]
        public string ProductName { get; set; }

        public override bool IsValid(ValidateType type)
        {
            throw new System.NotImplementedException();
        }

        public override string DeleteRule { get; set; } = "";
    }
}
