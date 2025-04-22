namespace HospitalApp.Models
{
    public class Doctor : User
    {
        public override string Role => "doctor";

        public string specialization{get; set;}
        public int is_available{get; set;}
    }
}
