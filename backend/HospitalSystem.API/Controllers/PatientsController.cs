using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using HospitalApp.Models;
using System;
using System.Threading.Tasks;

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
            string query = @"SELECT * from patients";

            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                patients.Add(new Patients
                {
                    PatientID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Sex = reader.GetString(2),
                    Address = reader.GetString(3),
                    BloodType = reader.GetString(4),
                    Email = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Bday = reader.GetDateTime(7)
                });
            }

            return Ok(patients);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving appointments: " + ex.Message);
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
                (name, sex, address, bloodtype, email, contactNumber, bday)
                VALUES ( @PatientName, @sex, @address, @bloodtype, @email, @contactnumber, @bday);";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PatientName", patient.Name.Trim());
            cmd.Parameters.AddWithValue("@sex", patient.Sex);
            cmd.Parameters.AddWithValue("@address", patient.Address);
            cmd.Parameters.AddWithValue("@bloodtype", patient.BloodType);
            cmd.Parameters.AddWithValue("@email", patient.Email.Trim());
            cmd.Parameters.AddWithValue("@contactnumber", patient.ContactNumber.Trim());
            cmd.Parameters.AddWithValue("@bday", patient.Bday);


            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("PatientCreated", patient);
                return Ok(new { message = "Patient added successfully." });
            }

            return StatusCode(500, "Failed to create appointment.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error creating appointment: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientbyId(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        var patients = new List<Patients>();

        try
        {
            string query = @"SELECT * FROM patients WHERE patientId = @id";

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
                    Bday = reader.GetDateTime(7)
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