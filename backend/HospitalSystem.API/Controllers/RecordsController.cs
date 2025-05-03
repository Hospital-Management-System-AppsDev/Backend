using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using HospitalApp.Models;
using System;
using System.Threading.Tasks;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class RecordsController : ControllerBase
{
    private readonly IHubContext<HospitalHub> _hubContext;
    private readonly DatabaseConnection _dbConnection;

    public RecordsController(DatabaseConnection dbConnection, IHubContext<HospitalHub> hubContext)
    {
        _dbConnection = dbConnection;
        _hubContext = hubContext;
    }

    [HttpGet("{patientId}")]
    public async Task<IActionResult> GetRecordsByPatient(int patientId)
    {
        try
        {
            var records = new List<Records>();
            
            using (var connection = _dbConnection.GetOpenConnection())
            {                
                string query = @"
                    SELECT 
                        r.id, r.medicalCertificate AS medicalCertificatePath, r.diagnosis AS diagnosisPath, 
                        r.prescription AS prescriptionPath, r.fkPatientId, r.fkAppointmentId, r.fkDoctorId,
                        p.patientId, p.name, p.sex, p.address, p.bloodtype, p.email, p.contactNumber, p.bday,
                        a.pkId, a.PatientID AS AppointmentPatientID, a.PatientName, a.AssignedDoctor, 
                        a.AppointmentType, a.Status, a.AppointmentDateTime,
                        u.name AS DoctorName, u.email AS DoctorEmail, u.contact_number AS DoctorContactNumber,
                        u.sex AS DoctorSex, u.birthday AS DoctorBirthday, u.username AS DoctorUsername,
                        d.specialization, d.is_available
                    FROM records r
                    JOIN patients p ON r.fkPatientId = p.patientId
                    JOIN appointments a ON r.fkAppointmentId = a.pkId
                    JOIN doctors d ON r.fkDoctorId = d.doctor_id
                    JOIN users u ON d.doctor_id = u.id
                    WHERE r.fkPatientId = @PatientId
                    ORDER BY a.AppointmentDateTime DESC";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PatientId", patientId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var record = new Records
                            {
                                id = reader.GetInt32("id"),
                                medicalCertificatePath = reader.IsDBNull(reader.GetOrdinal("medicalCertificatePath")) ? null : reader.GetString("medicalCertificatePath"),
                                prescriptionPath = reader.IsDBNull(reader.GetOrdinal("prescriptionPath")) ? null : reader.GetString("prescriptionPath"),
                                diagnosisPath = reader.IsDBNull(reader.GetOrdinal("diagnosisPath")) ? null : reader.GetString("diagnosisPath"),
                                patient = new Patients
                                {
                                    PatientID = reader.GetInt32("patientId"),
                                    Name = reader.GetString("name"),
                                    Sex = reader.GetString("sex"),
                                    Address = reader.GetString("address"),
                                    BloodType = reader.GetString("bloodtype"),
                                    Email = reader.GetString("email"),
                                    ContactNumber = reader.GetString("contactNumber"),
                                    Bday = reader.GetDateTime("bday")
                                },
                                appointment = new Appointment
                                {
                                    pkId = reader.GetInt32("pkId"),
                                    PatientID = reader.GetInt32("AppointmentPatientID"),
                                    PatientName = reader.GetString("PatientName"),
                                    AppointmentType = reader.GetString("AppointmentType"),
                                    Status = reader.GetInt32("Status"),
                                    AppointmentDateTime = reader.GetDateTime("AppointmentDateTime"),
                                    AssignedDoctor = new Doctor
                                    {
                                        Id = reader.GetInt32("AssignedDoctor"),
                                        Name = reader.GetString("DoctorName"),
                                        Email = reader.GetString("DoctorEmail"),
                                        specialization = reader.GetString("specialization"),
                                        is_available = reader.GetInt32("is_available"),
                                        Username = reader.GetString("DoctorUsername"),
                                        ContactNumber = reader.GetString("DoctorContactNumber"),
                                        Sex = reader.GetString("DoctorSex"),
                                        Birthday = reader.GetDateTime("DoctorBirthday")
                                    }
                                }
                            };
                            
                            records.Add(record);
                        }
                    }
                }
            }
            
            if (records.Count == 0)
            {
                return NotFound($"No records found for patient ID: {patientId}");
            }
            
            return Ok(records);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("add-record")]
    public async Task<IActionResult> AddRecord([FromBody] Records record){
        using var conn = _dbConnection.GetOpenConnection();

        try{
            string query = @"
                    INSERT INTO records
                    (medicalCertificate, diagnosis, prescription, fkPatientId, fkAppointmentId, fkDoctorId, date)
                    VALUES (@medicalCertificate, @diagnosis, @prescription, @fkPatientId, @fkAppointmentId, @fkDoctorId, @date);
            ";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@medicalCertificate", record.medicalCertificatePath.Trim() ?? "");
            cmd.Parameters.AddWithValue("@diagnosis", record.diagnosisPath.Trim() ?? "");
            cmd.Parameters.AddWithValue("@prescription", record.prescriptionPath.Trim() ?? "");
            cmd.Parameters.AddWithValue("@fkPatientId", record.patient.PatientID);
            cmd.Parameters.AddWithValue("@fkAppointmentId", record.appointment.pkId);
            cmd.Parameters.AddWithValue("@fkDoctorId", record.appointment.AssignedDoctor.Id);
            cmd.Parameters.AddWithValue("@date", record.appointment.AppointmentDateTime);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("RecordAdded", record);
                return Ok(new { message = "Appointment created successfully." });
            }

            return StatusCode(500, "Failed to create record.");

        }catch(Exception ex){
            return StatusCode(500, "Error creating record: " + ex.Message);
        }
    }

}
