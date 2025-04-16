using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using HospitalApp.Models;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase{
    private readonly IHubContext<HospitalHub> _hubContext;
    private readonly DatabaseConnection _dbConnection;

    public UsersController(DatabaseConnection dbConnection, IHubContext<HospitalHub> hubContext)
    {
        _dbConnection = dbConnection;
        _hubContext = hubContext;
    }

    [HttpPost("add/admin")]
    public async Task<IActionResult> AddAdmin([FromBody] User user){
        if(user == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Gender) || string.IsNullOrEmpty(user.ContactNumber) || string.IsNullOrEmpty(user.Role)){
            return BadRequest("Invalid User Data.");
        }

        using var conn = _dbConnection.GetOpenConnection();
        using var transaction= conn.BeginTransaction();

        try{
            string insertAdminQuery = @"INSERT INTO users(name, age, email, role, username, password, contact_number, gender) VALUES (@name, @age, @email, 'admin', @username, @password, @contact, @gender);";

            using var cmdUser = new MySqlCommand(insertAdminQuery, conn, transaction);
            cmdUser.Parameters.AddWithValue("@name", user.Name);
            cmdUser.Parameters.AddWithValue("@age", user.Age);
            cmdUser.Parameters.AddWithValue("@email", user.Email);
            cmdUser.Parameters.AddWithValue("@username", user.Username);
            cmdUser.Parameters.AddWithValue("@password",  BCrypt.Net.BCrypt.HashPassword(user.Password));
            cmdUser.Parameters.AddWithValue("@contact", user.ContactNumber);
            cmdUser.Parameters.AddWithValue("@gender", user.Gender);
            
            int resUser = cmdUser.ExecuteNonQuery();
            if (resUser <= 0)
            {
                transaction.Rollback();
                return StatusCode(500, "Failed to add admin");
            }
            int adminId = (int)cmdUser.LastInsertedId;
            transaction.Commit();

            var newAdmin = new{
                adminId,
                user.Name,
                user.Email,
                user.Username,
                user.Password,
                user.Gender,
                user.ContactNumber,
                user.Age
            };

            await _hubContext.Clients.All.SendAsync("AdminAdded", newAdmin);

            return Ok(new {Message = "Admin added successfully!", adminId});

        }catch (Exception ex){
            transaction.Rollback();
            return StatusCode(500, "Error adding admin: " + ex.Message);
        }
    }

    [HttpGet("by-username/{username}")]
    public IActionResult GetUserByUsername(string username) {
        using var conn = _dbConnection.GetOpenConnection();

        try {
            string query = "SELECT * FROM users WHERE username = @username;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();

            if (reader.Read()) {
                var user = new User {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Age = reader.GetInt32(2),
                    Email = reader.GetString(3),
                    Role = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    ContactNumber = reader.GetString(7),
                    Gender = reader.GetString(8) 
                };

                return Ok(user);
            }
            return NotFound("Username not found.");
        } catch (Exception ex) {
            return StatusCode(500, "Error retrieving user: " + ex.Message);
        }
    }

    [HttpGet("by-id/{id}")]
    public IActionResult GetUserById(int id){
        using var conn = _dbConnection.GetOpenConnection();

        try {
            string query = "SELECT * FROM users WHERE id = @id;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read()) {
                var user = new User {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Age = reader.GetInt32(2),
                    Email = reader.GetString(3),
                    Role = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    ContactNumber = reader.GetString(7),
                    Gender = reader.GetString(8) 
                };

                return Ok(user);
            }
            return NotFound("Username not found.");
        } catch (Exception ex) {
            return StatusCode(500, "Error retrieving user: " + ex.Message);
        }
    }

}