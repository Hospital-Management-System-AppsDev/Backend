using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    public decimal temperature { get; set; }
    public int pulseRate { get; set; }
    public decimal weight { get; set; }
    public decimal height { get; set; }
    public decimal sugarLevel { get; set; }

    [RegularExpression(@"^\d{2,3}/\d{2,3}$", ErrorMessage = "Blood pressure must be in the format 'mmHg/mmHg'")]
    public string bloodPressure { get; set; }

    public decimal bmi{
        get
        {
            decimal heightInMeters = height / 100; // Convert height from cm to meters
            return weight / (heightInMeters * heightInMeters);
        }
    }


    public string chiefComplaint{get; set;}
    public PatientMedicalInfo patientMedicalInfo { get; set; }
}
