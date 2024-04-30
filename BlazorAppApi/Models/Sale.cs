using System.ComponentModel.DataAnnotations.Schema;

namespace APICoches.Models
{
    public class Sale

    {
        public int Id { get; set; }
        public string Seller { get; set; }
        public float Price { get; set; }
        public int Kilometers { get; set; }
        public Car Car { get; set; }
    }
}
