using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using HospitalApp.Models;
using System;
using System.Threading.Tasks;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class PatientsController : ControllerBase
{
    private readonly IHubContext<HospitalHub> _hubContext;
    private readonly DatabaseConnection _dbConnection;

    public PatientsController(DatabaseConnection dbConnection, IHubContext<HospitalHub> hubContext)
    {
        _dbConnection = dbConnection;
        _hubContext = hubContext;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetPatients()
    {
        using var conn = _dbConnection.GetOpenConnection();
        var patients = new List<Patients>();

        try
        {
            string query = @"
                SELECT 
                    p.patientId, p.name, p.sex, p.address, p.bloodType, p.email, p.contactNumber, p.bday, p.profile_picture,
                    m.diet, m.exercise, m.sleep, m.smoking, m.alcohol, m.current_medications, 
                    m.medical_allergy, m.latex_allergy, m.food_allergy, m.other_allergy
                FROM patients p
                JOIN patient_med_info m ON p.patientId = m.patientId;";

            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var patient = new Patients
                {
                    PatientID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Sex = reader.GetString(2),
                    Address = reader.GetString(3),
                    BloodType = reader.GetString(4),
                    Email = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Bday = reader.GetDateTime(7),
                    ProfilePicture = reader.GetString(8),
                    PatientMedicalInfo = new PatientMedicalInfo
                    {
                        diet = reader.GetString(9),
                        exercise = reader.GetString(10),
                        sleep = reader.GetString(11),
                        smoking = reader.GetString(12),
                        alcohol = reader.GetString(13),
                        currentMedication = reader.GetString(14),
                        medicalAllergies = reader.GetString(15),
                        latexAllergy = reader.GetBoolean(16),
                        foodAllergy = reader.GetString(17),
                        otherAllergies = reader.GetString(18)
                    }
                };

                patients.Add(patient);
            }

            return Ok(patients);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving patient data: " + ex.Message);
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try{
            string query = @"DELETE FROM patients WHERE patientId = @id";
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)   
            {
                await _hubContext.Clients.All.SendAsync("PatientDeleted", id);
                return Ok(new { message = "Patient deleted successfully." });
            }
            return NotFound($"Patient with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error deleting patient: " + ex.Message);
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddPatient([FromBody] Patients patient)
    {
        using var conn =  _dbConnection.GetOpenConnection();

        try
        {
            string query = @"
                INSERT INTO patients 
                (name, sex, address, bloodtype, email, contactNumber, bday, profile_picture)
                VALUES ( @PatientName, @sex, @address, @bloodtype, @email, @contactnumber, @bday, @profile_picture);";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PatientName", patient.Name.Trim());
            cmd.Parameters.AddWithValue("@sex", patient.Sex);
            cmd.Parameters.AddWithValue("@address", patient.Address);
            cmd.Parameters.AddWithValue("@bloodtype", patient.BloodType);
            cmd.Parameters.AddWithValue("@email", patient.Email.Trim());
            cmd.Parameters.AddWithValue("@contactnumber", patient.ContactNumber.Trim());
            cmd.Parameters.AddWithValue("@bday", patient.Bday);
            cmd.Parameters.AddWithValue("@profile_picture", patient.ProfilePicture);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            int lastInsertedId = (int)cmd.LastInsertedId;

            string query2 = @"
                INSERT INTO patient_med_info (patientId, diet, exercise, sleep, smoking, alcohol, current_medications, medical_allergy, latex_allergy, food_allergy, other_allergy)
                VALUES (@patientId, @diet, @exercise, @sleep, @smoking, @alcohol, @current_medications, @medical_allergy, @latex_allergy, @food_allergy, @other_allergy);";

            await using var cmd2 = new MySqlCommand(query2, conn);
            cmd2.Parameters.AddWithValue("@patientId", lastInsertedId);
            cmd2.Parameters.AddWithValue("@diet", patient.PatientMedicalInfo.diet);
            cmd2.Parameters.AddWithValue("@exercise", patient.PatientMedicalInfo.exercise);
            cmd2.Parameters.AddWithValue("@sleep", patient.PatientMedicalInfo.sleep);
            cmd2.Parameters.AddWithValue("@smoking", patient.PatientMedicalInfo.smoking);
            cmd2.Parameters.AddWithValue("@alcohol", patient.PatientMedicalInfo.alcohol);
            cmd2.Parameters.AddWithValue("@current_medications", patient.PatientMedicalInfo.currentMedication.Replace(Environment.NewLine, ", "));
            cmd2.Parameters.AddWithValue("@medical_allergy", patient.PatientMedicalInfo.medicalAllergies.Replace(Environment.NewLine, ", "));
            cmd2.Parameters.AddWithValue("@latex_allergy", patient.PatientMedicalInfo.latexAllergy);
            cmd2.Parameters.AddWithValue("@food_allergy", patient.PatientMedicalInfo.foodAllergy.Replace(Environment.NewLine, ", "));
            cmd2.Parameters.AddWithValue("@other_allergy", patient.PatientMedicalInfo.otherAllergies.Replace(Environment.NewLine, ", "));

            int rowsAffected2 = await cmd2.ExecuteNonQueryAsync();

            if (rowsAffected > 0 && rowsAffected2 > 0)
            {
                await _hubContext.Clients.All.SendAsync("PatientCreated", patient);
                return Ok(new { message = "Patient added successfully." });
            }

            return StatusCode(500, "Failed to create patient.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error creating patient: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientbyId(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        var patients = new List<Patients>();

        try
        {
            string query = @"SELECT p.patientId, p.name, p.sex, p.address, p.bloodType, p.email, p.contactNumber, p.bday, p.profile_picture,
                    m.diet, m.exercise, m.sleep, m.smoking, m.alcohol, m.current_medications, 
                    m.medical_allergy, m.latex_allergy, m.food_allergy, m.other_allergy
                FROM patients p
                JOIN patient_med_info m ON p.patientId = m.patientId
                WHERE p.patientId = @id;";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
        
            if (await reader.ReadAsync())
            {
                Patients patient = new Patients
                {
                    PatientID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Sex = reader.GetString(2),
                    Address = reader.GetString(3),
                    BloodType = reader.GetString(4),
                    Email = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Bday = reader.GetDateTime(7),
                    ProfilePicture = reader.GetString(8),
                    PatientMedicalInfo = new PatientMedicalInfo
                    {
                        diet = reader.GetString(9),
                        exercise = reader.GetString(10),
                        sleep = reader.GetString(11),
                        smoking = reader.GetString(12),
                        alcohol = reader.GetString(13),
                        currentMedication = reader.GetString(14),
                        medicalAllergies = reader.GetString(15),
                        latexAllergy = reader.GetBoolean(16),
                        foodAllergy = reader.GetString(17),
                        otherAllergies = reader.GetString(18)
                    }
                };
                return Ok(patient);
            }else{
                return NotFound($"Patient with ID {id} not found.");
            }

        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving patient: " + ex.Message);
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] Patients patient)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try{
            string query = @"
                UPDATE patients
                SET name = @name, sex = @sex, address = @address, bloodtype = @bloodtype, email = @email, contactnumber = @contactnumber, bday = @bday, profile_picture = @profile_picture
                WHERE patientId = @id;";

            string query2 = @"
                UPDATE patient_med_info
                SET diet = @diet, exercise = @exercise, sleep = @sleep, smoking = @smoking, alcohol = @alcohol, current_medications = @current_medications, medical_allergy = @medical_allergy, latex_allergy = @latex_allergy, food_allergy = @food_allergy, other_allergy = @other_allergy
                WHERE patientId = @id;";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", patient.Name);
            cmd.Parameters.AddWithValue("@sex", patient.Sex);
            cmd.Parameters.AddWithValue("@address", patient.Address);
            cmd.Parameters.AddWithValue("@bloodtype", patient.BloodType);
            cmd.Parameters.AddWithValue("@email", patient.Email);
            cmd.Parameters.AddWithValue("@contactnumber", patient.ContactNumber);
            cmd.Parameters.AddWithValue("@bday", patient.Bday);
            cmd.Parameters.AddWithValue("@profile_picture", patient.ProfilePicture);
            await using var cmd2 = new MySqlCommand(query2, conn);
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.Parameters.AddWithValue("@diet", patient.PatientMedicalInfo.diet);
            cmd2.Parameters.AddWithValue("@exercise", patient.PatientMedicalInfo.exercise);
            cmd2.Parameters.AddWithValue("@sleep", patient.PatientMedicalInfo.sleep);
            cmd2.Parameters.AddWithValue("@smoking", patient.PatientMedicalInfo.smoking);
            cmd2.Parameters.AddWithValue("@alcohol", patient.PatientMedicalInfo.alcohol);
            cmd2.Parameters.AddWithValue("@current_medications", patient.PatientMedicalInfo.currentMedication.Replace(Environment.NewLine, ", "));
            cmd2.Parameters.AddWithValue("@medical_allergy", patient.PatientMedicalInfo.medicalAllergies.Replace(Environment.NewLine, ", "));
            cmd2.Parameters.AddWithValue("@latex_allergy", patient.PatientMedicalInfo.latexAllergy);
            cmd2.Parameters.AddWithValue("@food_allergy", patient.PatientMedicalInfo.foodAllergy.Replace(Environment.NewLine, ", "));
            cmd2.Parameters.AddWithValue("@other_allergy", patient.PatientMedicalInfo.otherAllergies.Replace(Environment.NewLine, ", "));

            int rowsAffected2 = await cmd2.ExecuteNonQueryAsync();

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0 && rowsAffected2 > 0)
            {
                await _hubContext.Clients.All.SendAsync("PatientUpdated", patient);
                return Ok(new { message = "Patient updated successfully." });
            }

            return StatusCode(500, "Failed to update patient.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error updating patient: " + ex.Message);
        }
        
    }

    [HttpGet("getnumpatientspermonth/{year:int}/{month:int}")]
    public async Task<IActionResult> GetNumPatientsPerMonth(int year, int month)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = @"
                SELECT MONTH(createdAt) AS Month, COUNT(PatientID) AS NumPatients
                FROM patients
                WHERE YEAR(createdAt) = @year AND MONTH(createdAt) <= @month
                GROUP BY MONTH(createdAt);";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@month", month);

            var result = new int[month]; // Index 0 = Jan, Index (month-1) = the given month

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int m = Convert.ToInt32(reader["Month"]);   // e.g., 1 for Jan, 2 for Feb
                int count = Convert.ToInt32(reader["NumPatients"]);
                result[m - 1] = count; // fill result at correct month index
            }
            return Ok(result.ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving patient count: " + ex.Message);
        }
    }

    [HttpGet("getnewpatients/{year}/{month}")]
    public async Task<IActionResult> GetNewDoctors(int year, int month)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string countQuery = @"
                SELECT COUNT(patientId) AS NumPatients
                FROM patients
                WHERE YEAR(createdAt) = @year AND MONTH(createdAt) = @month;";

            using var cmd = new MySqlCommand(countQuery, conn);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@month", month);

            var result = await cmd.ExecuteScalarAsync();
            int numDoctors = Convert.ToInt32(result);

            return Ok(numDoctors);
        }
        catch (Exception ex)
        {
            // Log the error (optional)
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("getnumpatients")]
    public async Task<IActionResult> GetNumDoctors()
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string countQuery = @"
                SELECT COUNT(patientId) AS NumPatients
                FROM patients;";

            using var cmd = new MySqlCommand(countQuery, conn);

            var result = await cmd.ExecuteScalarAsync();
            int numDoctors = Convert.ToInt32(result);

            return Ok(numDoctors);
        }
        catch (Exception ex)
        {
            // Log the error (optional)
            return StatusCode(500, new { error = ex.Message });
        }
    }
}