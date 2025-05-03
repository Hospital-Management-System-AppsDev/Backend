using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using HospitalApp.Models;
using System.Text.RegularExpressions;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IHubContext<HospitalHub> _hubContext;
    private readonly DatabaseConnection _dbConnection;

    public UsersController(DatabaseConnection dbConnection, IHubContext<HospitalHub> hubContext)
    {
        _dbConnection = dbConnection;
        _hubContext = hubContext;
    }

    [HttpPost("add/admin")]
    public async Task<IActionResult> AddAdmin([FromBody] User user)
    {
        if (user == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Sex) || string.IsNullOrEmpty(user.ContactNumber) || string.IsNullOrEmpty(user.Role))
        {
            return BadRequest("Invalid User Data.");
        }

        using var conn = _dbConnection.GetOpenConnection();
        using var transaction = conn.BeginTransaction();

        try
        {
            if (!Regex.IsMatch(user.Username, @"^[a-zA-Z0-9]+$"))
            {
                return BadRequest(new { message = "Username can only contain letters and numbers" });
            }

            if (user.Email.Contains(' '))
                return BadRequest(new { message = "Email should not contain spaces" });

            string insertAdminQuery = @"INSERT INTO users(name, birthday, email, role, username, password, contact_number, sex) 
            VALUES (@name, @birthday, @email, 'admin', @username, @password, @contact, @sex);";


            using var cmdUser = new MySqlCommand(insertAdminQuery, conn, transaction);
            cmdUser.Parameters.AddWithValue("@name", user.Name);
            cmdUser.Parameters.AddWithValue("@birthday", user.Birthday);
            cmdUser.Parameters.AddWithValue("@email", user.Email);
            cmdUser.Parameters.AddWithValue("@username", user.Username);
            cmdUser.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(user.Password));
            cmdUser.Parameters.AddWithValue("@contact", user.ContactNumber);
            cmdUser.Parameters.AddWithValue("@sex", user.Sex);

            int resUser = cmdUser.ExecuteNonQuery();
            if (resUser <= 0)
            {
                transaction.Rollback();
                return StatusCode(500, "Failed to add admin");
            }
            int adminId = (int)cmdUser.LastInsertedId;
            transaction.Commit();

            var newAdmin = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Username,
                user.ContactNumber,
                user.Sex,
                user.Birthday,
                user.Age
            };

            await _hubContext.Clients.All.SendAsync("AdminAdded", newAdmin);

            return Ok(new { Message = "Admin added successfully!", adminId });

        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, "Error adding admin: " + ex.Message);
        }
    }

    [HttpGet("by-username/{username}")]
    public IActionResult GetUserByUsername(string username)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = "SELECT * FROM users WHERE username = @username;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = reader.GetString(3),
                    Username = reader.GetString(4),
                    Password = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Sex = reader.GetString(7),
                    Birthday = reader.GetDateTime(8)
                };


                return Ok(user);
            }
            return NotFound("Username not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving user: " + ex.Message);
        }
    }

    [HttpGet("by-id/{id}")]
    public IActionResult GetUserById(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = "SELECT * FROM users WHERE id = @id;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = reader.GetString(3),
                    Username = reader.GetString(4),
                    Password = reader.GetString(5),
                    ContactNumber = reader.GetString(6),
                    Sex = reader.GetString(7),
                    Birthday = reader.GetDateTime(8)
                };


                return Ok(user);
            }
            return NotFound("Username not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving user: " + ex.Message);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest(new { message = "Name and email are required fields" });
            }
            if (user.Username.Contains(' '))
                return BadRequest(new { message = "Username should not contain spaces" });

            if (user.Email.Contains(' '))
                return BadRequest(new { message = "Email should not contain spaces" });

            // Validate email format
            if (!IsValidEmail(user.Email))
            {
                return BadRequest(new { message = "Invalid email format" });
            }

            using var conn = _dbConnection.GetOpenConnection();
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string insertAdminQuery = @"INSERT INTO users(name, birthday, email, role, username, password, contact_number, sex) 
                VALUES (@name, @birthday, @email, 'admin', @username, @password, @contact, @sex);";

                using var cmdUser = new MySqlCommand(insertAdminQuery, conn, transaction);
                cmdUser.Parameters.AddWithValue("@name", user.Name);
                cmdUser.Parameters.AddWithValue("@birthday", user.Birthday);
                cmdUser.Parameters.AddWithValue("@email", user.Email);
                cmdUser.Parameters.AddWithValue("@username", user.Username);
                cmdUser.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(user.Password));
                cmdUser.Parameters.AddWithValue("@contact", user.ContactNumber);
                cmdUser.Parameters.AddWithValue("@sex", user.Sex);

                int resUser = cmdUser.ExecuteNonQuery();
                if (resUser <= 0)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Failed to add admin");
                }
                int adminId = (int)cmdUser.LastInsertedId;
                await transaction.CommitAsync();

                var newAdmin = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.Username,
                    user.ContactNumber,
                    user.Sex,
                    user.Birthday,
                    user.Age
                };

                await _hubContext.Clients.All.SendAsync("AdminAdded", newAdmin);

                return Ok(new { Message = "Admin added successfully!", adminId });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}