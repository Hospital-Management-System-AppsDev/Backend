namespace HospitalApp.Models{
    public class PharmacyTranscations{
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }

        public string ReceiptPath { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}