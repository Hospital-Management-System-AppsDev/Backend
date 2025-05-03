namespace HospitalApp.Models{
    public class Medicines{
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stocks { get; set; }
        public string Manufacturer { get; set; }
        public string Type { get; set; }
        public decimal Dosage { get; set; }
        public string Unit { get; set; }
    }
}