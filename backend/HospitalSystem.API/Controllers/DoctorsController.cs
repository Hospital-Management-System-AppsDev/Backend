using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using BCrypt.Net;


[Route("api/[controller]")]
[ApiController]
public class DoctorsController : ControllerBase
{
    private readonly IHubContext<DoctorHub> _hubContext;
    private readonly DatabaseConnection _dbConnection;

    public DoctorsController(DatabaseConnection dbConnection, IHubContext<DoctorHub> hubContext)
    {
        _dbConnection = dbConnection;
        _hubContext = hubContext;
    }

    // ✅ Get All Doctors
    [HttpGet]
    public IActionResult GetDoctors()
    {
        using var conn = _dbConnection.GetOpenConnection();
        var doctors = new List<Doctor>();

        try
        {
            string query = "SELECT users.id, users.name, users.email, users.username, users.password, users.gender, users.contact_number, users.age, doctors.specialization, doctors.is_available FROM users JOIN doctors ON users.id = doctors.doctor_id WHERE users.role = 'doctor';";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                doctors.Add(new Doctor
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Username = reader.GetString(3),
                    Password = reader.GetString(4),
                    Gender = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Age = reader.GetInt32(7),
                    specialization = reader.GetString(8),
                    is_available = reader.GetInt16(9)
                });
            }
            return Ok(doctors);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving doctors: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetDoctorById(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = @"
                SELECT users.id, users.name, users.email, users.username, users.password, 
                    users.gender, users.contact_number, users.age, doctors.specialization, 
                    doctors.is_available 
                FROM users 
                JOIN doctors ON users.id = doctors.doctor_id 
                WHERE users.role = 'doctor' AND users.id = @id;"; // Added WHERE users.id = @id

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id); // Prevents SQL injection

            using var reader = cmd.ExecuteReader();
            
            if (reader.Read()) // Fetch single doctor
            {
                var doctor = new Doctor
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Username = reader.GetString(3),
                    Password = reader.GetString(4),
                    Gender = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Age = reader.GetInt32(7),
                    specialization = reader.GetString(8),
                    is_available = reader.GetInt16(9)
                };
                bool isMatch = BCrypt.Net.BCrypt.Verify("123", doctor.Password);
                Console.WriteLine($"Password matches hash {doctor.Password}: {isMatch}");

                return Ok(doctor);
            }
            else
            {
                return NotFound($"Doctor with ID {id} not found.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving doctor: " + ex.Message);
        }
    }


    [HttpPatch("{id}/availability")]
    public async Task<IActionResult> UpdateDoctorAvailability(int id, [FromBody] int isAvailable)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string updateQuery = "UPDATE doctors SET is_available = @isAvailable WHERE doctor_id = @id";
            using var cmd = new MySqlCommand(updateQuery, conn);
            cmd.Parameters.AddWithValue("@isAvailable", isAvailable);
            cmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                // ✅ Notify all clients via SignalR
                await _hubContext.Clients.All.SendAsync("UpdateDoctorAvailability", id, isAvailable);
                return Ok("Doctor availability updated successfully");
            }
            return StatusCode(500, "Failed to update availability");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error updating availability: " + ex.Message);
        }
    }

    // ✅ Add a New Doctor
    [HttpPost("add")]
    public async Task<IActionResult> AddDoctor([FromBody] Doctor doctor)
    {
        if (doctor == null || string.IsNullOrEmpty(doctor.Name) || string.IsNullOrEmpty(doctor.Email) ||
            string.IsNullOrEmpty(doctor.specialization) || string.IsNullOrEmpty(doctor.Username) ||
            string.IsNullOrEmpty(doctor.Password) || string.IsNullOrEmpty(doctor.Gender) ||
            string.IsNullOrEmpty(doctor.ContactNumber))
        {
            return BadRequest("Invalid doctor data");
        }

        using var conn = _dbConnection.GetOpenConnection();
        using var transaction = conn.BeginTransaction();

        try
        {
            // ✅ Insert into `users` table
            string insertUserQuery = @"INSERT INTO users (name, age, email, role, username, password, 
                                       contact_number, gender) 
                                       VALUES(@name, @age, @email, 'doctor', @username, @password, 
                                       @contact, @gender)";

            using var cmdUser = new MySqlCommand(insertUserQuery, conn, transaction);
            cmdUser.Parameters.AddWithValue("@name", doctor.Name);
            cmdUser.Parameters.AddWithValue("@age", doctor.Age);
            cmdUser.Parameters.AddWithValue("@email", doctor.Email);
            cmdUser.Parameters.AddWithValue("@username", doctor.Username);
            cmdUser.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(doctor.Password));  // ⚠️ Consider hashing the password
            cmdUser.Parameters.AddWithValue("@contact", doctor.ContactNumber);
            cmdUser.Parameters.AddWithValue("@gender", doctor.Gender);

            int resUser = cmdUser.ExecuteNonQuery();
            if (resUser <= 0)
            {
                transaction.Rollback();
                return StatusCode(500, "Failed to add doctor");
            }

            int doctorId = (int)cmdUser.LastInsertedId;

            // ✅ Insert into `doctor_availability` table
            string insertAvailabilityQuery = @"INSERT INTO doctors (doctor_id, specialization, is_available) 
                                               VALUES(@doctorId, @specialization, 1)";

            using var cmdAvailability = new MySqlCommand(insertAvailabilityQuery, conn, transaction);
            cmdAvailability.Parameters.AddWithValue("@doctorId", doctorId);
            cmdAvailability.Parameters.AddWithValue("@specialization", doctor.specialization);

            int resAvailability = cmdAvailability.ExecuteNonQuery();
            if (resAvailability <= 0)
            {
                transaction.Rollback();
                return StatusCode(500, "Failed to set doctor availability");
            }

            transaction.Commit();

            var newDoctor = new
            {
                doctorId,
                doctor.Name,
                doctor.Age,
                doctor.Email,
                doctor.Username,
                doctor.ContactNumber,
                doctor.Gender,
                doctor.specialization,
                is_available = 1
            };

            // ✅ Notify clients via SignalR
            await _hubContext.Clients.All.SendAsync("DoctorAdded", newDoctor);

            return Ok(new { Message = "Doctor added successfully!", doctorId });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, "Error adding doctor: " + ex.Message);
        }
    }
}
