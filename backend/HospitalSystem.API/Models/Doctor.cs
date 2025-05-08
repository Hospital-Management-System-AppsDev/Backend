namespace HospitalApp.Models
{
    public class Doctor : User
    {
        public override string Role => "doctor";

        public string specialization{get; set;}
        public int is_available{get; set;}
        public string profile_picture{get; set;}
        public string signature{get; set;}
    }
}
