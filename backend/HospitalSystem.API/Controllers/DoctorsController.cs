using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using BCrypt.Net;
using HospitalApp.Models;
using System.Text.RegularExpressions;

[Route("api/[controller]")]
[ApiController]
public class DoctorsController : ControllerBase
{
    private readonly IHubContext<HospitalHub> _hubContext;
    private readonly DatabaseConnection _dbConnection;

    public DoctorsController(DatabaseConnection dbConnection, IHubContext<HospitalHub> hubContext)
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
            string query = "SELECT users.id, users.name, users.email, users.username, users.password, users.sex, users.contact_number, users.birthday, doctors.specialization, doctors.is_available, doctors.profile_picture, doctors.signature FROM users JOIN doctors ON users.id = doctors.doctor_id WHERE users.role = 'doctor';";
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
                    Sex = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Birthday = reader.GetDateTime(7),
                    specialization = reader.GetString(8),
                    is_available = reader.GetInt16(9),
                    profile_picture = reader.GetString(10),
                    signature = reader.GetString(11)
                });
            }
            return Ok(doctors);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving doctors: " + ex.Message);
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string deleteQuery = @"DELETE FROM users WHERE id = @id";
            using var cmd = new MySqlCommand(deleteQuery, conn);
            cmd.Parameters.AddWithValue("@id", id);
            
            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("DoctorDeleted", id);
                return Ok(new { Message = "Doctor deleted successfully!" });
            }
            return NotFound($"Doctor with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error deleting doctor: " + ex.Message);
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateDoctor(int id, [FromBody] Doctor doctor)
    {
        // Since you confirmed IDs match, we can proceed
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            // First, update the user in the database
            string updateQuery = @"UPDATE users SET 
                                email = @email, 
                                contact_number = @contact_number, 
                                birthday = @birthday 
                                WHERE id = @id";
            
            using var cmd = new MySqlCommand(updateQuery, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@email", doctor.Email);
            cmd.Parameters.AddWithValue("@contact_number", doctor.ContactNumber);
            cmd.Parameters.AddWithValue("@birthday", doctor.Birthday);
            
            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                // Notify clients about the update
                await _hubContext.Clients.All.SendAsync("DoctorUpdated", doctor);
                
                // Get the updated doctor from database to return
                string selectQuery = "SELECT * FROM users WHERE id = @id";
                using var selectCmd = new MySqlCommand(selectQuery, conn);
                selectCmd.Parameters.AddWithValue("@id", id);
                
                using var reader = selectCmd.ExecuteReader();
                if (reader.Read())
                {
                    // Map database fields to Doctor object
                    var updatedDoctor = new Doctor
                    {
                        Id = id,
                        Email = reader["email"].ToString(),
                        ContactNumber = reader["contact_number"].ToString(),
                        Birthday = Convert.ToDateTime(reader["birthday"]),
                        Name = doctor.Name,
                        Username = doctor.Username,
                        Password = doctor.Password,
                        Sex = doctor.Sex,
                        specialization = doctor.specialization,
                        is_available = doctor.is_available,
                        profile_picture = doctor.profile_picture,
                        signature = doctor.signature
                    };
                    
                    return Ok(updatedDoctor);
                }
                
                return Ok(doctor); // Fallback to returning the input doctor
            }
            
            return NotFound($"Doctor with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error updating doctor: " + ex.Message);
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
                    users.sex, users.contact_number, users.birthday, doctors.specialization, 
                    doctors.is_available, doctors.profile_picture, doctors.signature
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
                    Sex = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Birthday = reader.GetDateTime(7),
                    specialization = reader.GetString(8),
                    is_available = reader.GetInt16(9),
                    profile_picture = reader.GetString(10),
                    signature = reader.GetString(11)
                };

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

    [HttpGet("getnewdoctors/{year}/{month}")]
    public async Task<IActionResult> GetNewDoctors(int year, int month)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string countQuery = @"
                SELECT COUNT(id) AS NumDoctors
                FROM users
                WHERE YEAR(created_At) = @year AND MONTH(created_At) = @month AND role = 'doctor';";

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

    [HttpGet("getnumdoctors")]
    public async Task<IActionResult> GetNumDoctors()
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string countQuery = @"
                SELECT COUNT(id) AS NumDoctors
                FROM users
                WHERE role = 'doctor';";

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



    [HttpPatch("{id}/availability")]
    public async Task<IActionResult> UpdateDoctorAvailability(int id, [FromBody] int isAvailable)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string updateQuery = @"UPDATE doctors SET is_available = @isAvailable WHERE doctor_id = @id";
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
            string.IsNullOrEmpty(doctor.Password) || string.IsNullOrEmpty(doctor.Sex) ||
            string.IsNullOrEmpty(doctor.ContactNumber))
        {
            return BadRequest("Invalid doctor data");
        }

        if (doctor.Username.Contains(' '))
            return BadRequest(new { message = "Username should not contain spaces" });
        else if (!Regex.IsMatch(doctor.Username, @"^[a-zA-Z0-9]+$"))
        {
            return BadRequest(new { message = "Username can only contain letters and numbers" });
        }

        if (doctor.Email.Contains(' '))
            return BadRequest(new { message = "Email should not contain spaces" });


        using var conn = _dbConnection.GetOpenConnection();
        using var transaction = conn.BeginTransaction();

        try
        {

            // ✅ Insert into `users` table
            string insertUserQuery = @"INSERT INTO users (name, birthday, email, role, username, password, 
                                       contact_number, sex) 
                                       VALUES(@name, @birthday, @email, 'doctor', @username, @password, 
                                       @contact, @sex)";

            using var cmdUser = new MySqlCommand(insertUserQuery, conn, transaction);
            cmdUser.Parameters.AddWithValue("@name", doctor.Name);
            cmdUser.Parameters.AddWithValue("@birthday", doctor.Birthday);
            cmdUser.Parameters.AddWithValue("@email", doctor.Email);
            cmdUser.Parameters.AddWithValue("@username", doctor.Username);
            cmdUser.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(doctor.Password));  // ⚠️ Consider hashing the password
            cmdUser.Parameters.AddWithValue("@contact", doctor.ContactNumber);
            cmdUser.Parameters.AddWithValue("@sex", doctor.Sex);

            int resUser = cmdUser.ExecuteNonQuery();
            if (resUser <= 0)
            {
                transaction.Rollback();
                return StatusCode(500, "Failed to add doctor");
            }

            int doctorId = (int)cmdUser.LastInsertedId;

            // ✅ Insert into `doctor_availability` table
            string insertAvailabilityQuery = @"INSERT INTO doctors (doctor_id, specialization, is_available, profile_picture, signature) 
                                               VALUES(@doctorId, @specialization, 1, @profile_picture, @signature)";

            using var cmdAvailability = new MySqlCommand(insertAvailabilityQuery, conn, transaction);
            cmdAvailability.Parameters.AddWithValue("@doctorId", doctorId);
            cmdAvailability.Parameters.AddWithValue("@specialization", doctor.specialization);
            cmdAvailability.Parameters.AddWithValue("@profile_picture", doctor.profile_picture);
            cmdAvailability.Parameters.AddWithValue("@signature", doctor.signature);

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
                doctor.Birthday,
                doctor.Email,
                doctor.Username,
                doctor.ContactNumber,
                doctor.Sex,
                doctor.specialization,
                is_available = 1,
                doctor.profile_picture,
                doctor.signature
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
