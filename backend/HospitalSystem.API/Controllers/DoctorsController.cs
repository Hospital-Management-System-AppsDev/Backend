using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;



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
            string query = "SELECT users.id, users.name, users.email, doctor_availability.specialization, doctor_availability.is_available FROM users JOIN doctor_availability ON users.id = doctor_availability.doctor_id WHERE users.role = 'doctor';";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                doctors.Add(new Doctor
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    specialization = reader.GetString(3),
                    is_available = reader.GetInt32(4)
                });
            }
            return Ok(doctors);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving doctors: " + ex.Message);
        }
    }

    // ✅ Get Doctor by ID
//     [HttpGet("{id}")]
//     public IActionResult GetDoctorById(int id)
//     {
//         using var conn = _dbConnection.GetOpenConnection();
//         try
//         {
//             string query = "SELECT id, name, age, specialty FROM doctors WHERE id = @id";
//             using var cmd = new MySqlCommand(query, conn);
//             cmd.Parameters.AddWithValue("@id", id);
//             using var reader = cmd.ExecuteReader();

//             if (reader.Read())
//             {
//                 return Ok(new Doctor
//                 {
//                     Id = reader.GetInt32(0),
//                     Name = reader.GetString(1),
//                     Age = reader.GetInt32(2),
//                     Specialty = reader.GetString(3)
//                 });
//             }
//             return NotFound("Doctor not found");
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, "Error retrieving doctor: " + ex.Message);
//         }
//     }

//     // ✅ Get Doctors by Specialty (returns a list)
//     [HttpGet("by-specialty/{specialty}")]
//     public IActionResult GetDoctorsBySpecialty(string specialty)
//     {
//         using var conn = _dbConnection.GetOpenConnection();
//         var doctors = new List<Doctor>();

//         try
//         {
//             string query = "SELECT id, name, age, specialty FROM doctors WHERE specialty = @specialty";
//             using var cmd = new MySqlCommand(query, conn);
//             cmd.Parameters.AddWithValue("@specialty", specialty);
//             using var reader = cmd.ExecuteReader();

//             while (reader.Read())
//             {
//                 doctors.Add(new Doctor
//                 {
//                     Id = reader.GetInt32(0),
//                     Name = reader.GetString(1),
//                     Age = reader.GetInt32(2),
//                     Specialty = reader.GetString(3)
//                 });
//             }
//             return doctors.Count > 0 ? Ok(doctors) : NotFound("No doctors found in this specialty.");
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, "Error retrieving doctors: " + ex.Message);
//         }
//     }

//     // ✅ Add a New Doctor
//     [HttpPost("add")]
//     public IActionResult AddDoctor([FromBody] Doctor doctor)
//     {
//         if (doctor == null || string.IsNullOrEmpty(doctor.Name) || string.IsNullOrEmpty(doctor.Specialty) || doctor.Age <= 0)
//         {
//             return BadRequest("Invalid doctor data");
//         }

//         using var conn = _dbConnection.GetOpenConnection();
//         try
//         {
//             string query = "INSERT INTO doctors (name, age, specialty) VALUES(@name, @age, @specialty)";
//             using var cmd = new MySqlCommand(query, conn);
//             cmd.Parameters.AddWithValue("@name", doctor.Name);
//             cmd.Parameters.AddWithValue("@age", doctor.Age);
//             cmd.Parameters.AddWithValue("@specialty", doctor.Specialty);

//             int res = cmd.ExecuteNonQuery();
//             return res > 0 ? Ok("Doctor added successfully") : StatusCode(500, "Failed to add doctor");
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, "Error adding doctor: " + ex.Message);
//         }
//     }

//     // ✅ Update Doctor Age
//     [HttpPatch("{id}/age")]
//     public IActionResult UpdateAge(int id, [FromBody] int age)
//     {
//         if (age <= 0)
//         {
//             return BadRequest("Invalid age value");
//         }

//         using var conn = _dbConnection.GetOpenConnection();
//         try
//         {
//             string checkQuery = "SELECT COUNT(*) FROM doctors WHERE id = @id";
//             using var checkCmd = new MySqlCommand(checkQuery, conn);
//             checkCmd.Parameters.AddWithValue("@id", id);
//             bool exists = (int)(checkCmd.ExecuteScalar() ?? 0) > 0;

//             if (!exists) return NotFound("Doctor not found");

//             string updateQuery = "UPDATE doctors SET age = @age WHERE id = @id";
//             using var updateCmd = new MySqlCommand(updateQuery, conn);
//             updateCmd.Parameters.AddWithValue("@age", age);
//             updateCmd.Parameters.AddWithValue("@id", id);

//             int rowsAffected = updateCmd.ExecuteNonQuery();
//             return rowsAffected > 0 ? Ok("Doctor's age updated successfully") : StatusCode(500, "Failed to update age");
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, "Error updating age: " + ex.Message);
//         }
//     }

//     // ✅ Update Doctor Name
//     [HttpPatch("{id}/name")]
//     public IActionResult UpdateName(int id, [FromBody] string name)
//     {
//         if (string.IsNullOrEmpty(name))
//         {
//             return BadRequest("Invalid name value");
//         }

//         using var conn = _dbConnection.GetOpenConnection();
//         try
//         {
//             string checkQuery = "SELECT COUNT(*) FROM doctors WHERE id = @id";
//             using var checkCmd = new MySqlCommand(checkQuery, conn);
//             checkCmd.Parameters.AddWithValue("@id", id);
//             bool exists = (int)(checkCmd.ExecuteScalar() ?? 0) > 0;

//             if (!exists) return NotFound("Doctor not found");

//             string updateQuery = "UPDATE doctors SET name = @name WHERE id = @id";
//             using var updateCmd = new MySqlCommand(updateQuery, conn);
//             updateCmd.Parameters.AddWithValue("@name", name);
//             updateCmd.Parameters.AddWithValue("@id", id);

//             int rowsAffected = updateCmd.ExecuteNonQuery();
//             return rowsAffected > 0 ? Ok("Doctor's name updated successfully") : StatusCode(500, "Failed to update name");
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, "Error updating name: " + ex.Message);
//         }
//     }

    [HttpPatch("{id}/availability")]
    public async Task<IActionResult> UpdateDoctorAvailability(int id, [FromBody] int isAvailable)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string updateQuery = "UPDATE doctor_availability SET is_available = @isAvailable WHERE doctor_id = @id";
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
}
