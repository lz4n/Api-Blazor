namespace APICoches.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public List<Sale> Sales { get; set; }
    }
}
