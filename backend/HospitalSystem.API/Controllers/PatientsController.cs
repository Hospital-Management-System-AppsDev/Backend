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
            cmd.Parameters.AddWithValue("@PatientName", patient.Name);
            cmd.Parameters.AddWithValue("@sex", patient.Sex);
            cmd.Parameters.AddWithValue("@address", patient.Address);
            cmd.Parameters.AddWithValue("@bloodtype", patient.BloodType);
            cmd.Parameters.AddWithValue("@email", patient.Email);
            cmd.Parameters.AddWithValue("@contactnumber", patient.ContactNumber);
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
}
