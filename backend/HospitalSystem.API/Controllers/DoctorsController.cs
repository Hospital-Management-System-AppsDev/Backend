using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class DoctorsController : ControllerBase
{
    private readonly DatabaseConnection _dbConnection;

    //Inject DatabaseConnection via DI
    public DoctorsController(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    [HttpGet]
    public IActionResult GetDoctors()
    {
        using var conn = _dbConnection.GetOpenConnection(); //Get an Open Connection
        var doctors = new List<object>();

        try
        {
            string query = "SELECT id, name, specialty FROM doctors";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                doctors.Add(new
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Specialty = reader.GetString(2)
                });
            }
            return Ok(doctors);
        }
        catch (Exception ex)
        {
            return StatusCode(500, " Error retrieving doctors: " + ex.Message);
        }
    }

    [HttpGet("{id}")] // Route: api/doctors/{id}
    public IActionResult GetDoctorById(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string query = "SELECT id, name, specialty FROM doctors WHERE id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read()) //Check if a doctor is found
            {
                var doctor = new
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Specialty = reader.GetString(2)
                };
                return Ok(doctor);
            }
            else
            {
                return NotFound("Doctor not found");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving doctor: " + ex.Message);
        }
    }

    [HttpGet("by-name/{name}")] // Route: api/doctors/by-name/{name}
    public IActionResult GetDoctorByName(string name)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string query = "SELECT id, name, specialty FROM doctors WHERE name = @name";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var doctor = new
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Specialty = reader.GetString(2)
                };
                return Ok(doctor);
            }
            else
            {
                return NotFound("Doctor not found");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving doctor: " + ex.Message);
        }
    }

    [HttpGet("by-specialty/{specialty}")]
    public IActionResult GetDoctorBySpecialty(string specialty){
        using var conn = _dbConnection.GetOpenConnection();
        try{
            string query = "SELECT * FROM doctors WHERE specialty = @specialty";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@specialty", specialty);

            using var reader = cmd.ExecuteReader();

            if(reader.Read()){
                var doctor = new {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Specialty = reader.GetString(3)
                };
                return Ok(doctor);
            }else{
                return NotFound("Doctor not found");
            }
        } catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving doctor: " + ex.Message);
        }
    }

    [HttpPost("add")]
    public IActionResult AddDoctor([FromBody] Doctor doctor)
    {
        if (doctor == null || string.IsNullOrEmpty(doctor.Name) || string.IsNullOrEmpty(doctor.Specialty))
        {
            return BadRequest("Invalid doctor data");
        }

        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string query = "INSERT INTO doctors (name, age, specialty) VALUES(@name, @age, @specialty)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", doctor.Name);
            cmd.Parameters.AddWithValue("@age", doctor.Age); // ➕ Add Age
            cmd.Parameters.AddWithValue("@specialty", doctor.Specialty);

            int res = cmd.ExecuteNonQuery();

            if (res > 0)
            {
                return Ok("Doctor added successfully");
            }
            return StatusCode(500, "Failed to add doctor");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error adding doctor: " + ex.Message);
        }
    }

    [HttpPatch("{id}/age")]
    public IActionResult UpdateAge(int id, [FromBody] int age)
    {
        if (age <= 0)
        {
            return BadRequest("Invalid age value");
        }

        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            // Check if doctor exists
            string checkQuery = "SELECT COUNT(*) FROM doctors WHERE id = @id";
            using var checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (exists == 0)
            {
                return NotFound("Doctor not found");
            }

            // Update only the age field
            string updateQuery = "UPDATE doctors SET age = @age WHERE id = @id";
            using var updateCmd = new MySqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@age", age);
            updateCmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = updateCmd.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                return Ok("Doctor's age updated successfully");
            }
            return StatusCode(500, "Failed to update age");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error updating age: " + ex.Message);
        }
    }

    [HttpPatch("{id}/name")]
    public IActionResult UpdateName(int id, [FromBody] string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Invalid age value");
        }

        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            // Check if doctor exists
            string checkQuery = "SELECT COUNT(*) FROM doctors WHERE id = @id";
            using var checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (exists == 0)
            {
                return NotFound("Doctor not found");
            }

            // Update only the age field
            string updateQuery = "UPDATE doctors SET name = @name WHERE id = @id";
            using var updateCmd = new MySqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@name", name);
            updateCmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = updateCmd.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                return Ok("Doctor's age updated successfully");
            }
            return StatusCode(500, "Failed to update age");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error updating age: " + ex.Message);
        }
    }

}
