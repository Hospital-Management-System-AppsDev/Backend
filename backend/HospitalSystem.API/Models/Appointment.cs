using System;
using System.Collections.Generic;

namespace HospitalApp.Models;

public class Appointment
{   
    public int pkId{ get; set; }
    public int PatientID { get; set; }
    public string PatientName { get; set; }
    public Doctor AssignedDoctor { get; set; }
    public string AppointmentType { get; set; }
    public int Status { get; set; } // 0 = pending, 1 = completed, etc.
    public DateTime AppointmentDateTime { get; set; } // Add this
}
