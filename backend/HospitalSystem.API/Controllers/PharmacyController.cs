using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using HospitalApp.Models;
using System.Threading.Tasks;
using System;
using MySql.Data.MySqlClient;

[Route("api/[controller]")]
[ApiController]
public class PharmacyController : ControllerBase
{
    private readonly IHubContext<HospitalHub> _hubContext;
    private readonly DatabaseConnection _dbConnection;

    public PharmacyController(DatabaseConnection dbConnection, IHubContext<HospitalHub> hubContext)
    {
        _dbConnection = dbConnection;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetPharmacy()
    {
        using var conn = _dbConnection.GetOpenConnection();
        var pharmacy = new List<Medicines>();

        try
        {
            var query = "SELECT * FROM pharmacy";
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pharmacy.Add(new Medicines
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Stocks = reader.GetInt32(3),
                    Manufacturer = reader.GetString(4),
                    Type = reader.GetString(5),
                    Dosage = reader.GetDecimal(6),
                    Unit = reader.GetString(7)
                });
            }
            return Ok(pharmacy);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicineById(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        var medicine = new Medicines();
        try
        {
            var query = "SELECT * FROM pharmacy WHERE id = @id";
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                medicine = new Medicines
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Stocks = reader.GetInt32(3),
                    Manufacturer = reader.GetString(4),
                    Type = reader.GetString(5),
                    Dosage = reader.GetDecimal(6),
                    Unit = reader.GetString(7)
                };
            }
            return Ok(medicine);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddMedicine(Medicines medicine)
    {
        using var conn = _dbConnection.GetOpenConnection();
        
        try{
            var query = "INSERT INTO pharmacy (medicine, price, stocks, manufacturer, type, dosage, unit) VALUES (@medicine, @price, @stocks, @manufacturer, @type, @dosage, @unit); SELECT LAST_INSERT_ID();";
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@medicine", medicine.Name.Trim());
            cmd.Parameters.AddWithValue("@price", medicine.Price);
            cmd.Parameters.AddWithValue("@stocks", medicine.Stocks);
            cmd.Parameters.AddWithValue("@manufacturer", medicine.Manufacturer.Trim());
            cmd.Parameters.AddWithValue("@type", medicine.Type.Trim());
            cmd.Parameters.AddWithValue("@dosage", medicine.Dosage);
            cmd.Parameters.AddWithValue("@unit", medicine.Unit.Trim());
            
            // Get the newly inserted ID
            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            
            if (newId > 0)
            {
                // Set the ID on the medicine object
                medicine.Id = newId;
                
                // Notify clients via SignalR
                await _hubContext.Clients.All.SendAsync("MedicineAdded", medicine);
                
                // Return the complete medicine object with ID
                return Ok(medicine);
            }
            return StatusCode(500, "Failed to add medicine.");
            
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicine(Medicines medicine)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            var query = "UPDATE pharmacy SET stocks = @stocks, price = @price WHERE id = @id";
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@stocks", medicine.Stocks);
            cmd.Parameters.AddWithValue("@price", medicine.Price);
            cmd.Parameters.AddWithValue("@id", medicine.Id);
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("MedicineUpdated", medicine);
                return Ok(medicine);
            }
            return StatusCode(500, "Failed to update medicine.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicine(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            var query = "DELETE FROM pharmacy WHERE id = @id";
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("MedicineDeleted", id);
                return Ok();
            }
            return StatusCode(500, "Failed to delete medicine.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}