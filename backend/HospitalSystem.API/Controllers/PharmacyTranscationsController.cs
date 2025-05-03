using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using HospitalApp.Models;
using System.Threading.Tasks;
using System;
using MySql.Data.MySqlClient;

[Route("api/[controller]")]
[ApiController]
public class PharmacyTranscationsController : ControllerBase
{
    private readonly DatabaseConnection _dbConnection;
    private readonly IHubContext<HospitalHub> _hubContext;

    public PharmacyTranscationsController(DatabaseConnection dbConnection, IHubContext<HospitalHub> hubContext)
    {
        _dbConnection = dbConnection;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetPharmacyTranscations()
    {
        using var conn = _dbConnection.GetOpenConnection();
        var transcations = new List<PharmacyTranscations>();

        try
        {
            var query = "SELECT * FROM pharmacy_transcations";
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                transcations.Add(new PharmacyTranscations
                {
                    Id = reader.GetInt32(0),
                    TotalAmount = reader.GetDecimal(1),
                    ReceiptPath = reader.GetString(2),
                    TransactionDate = reader.GetDateTime(3)
                });
            }
            return Ok(transcations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPharmacyTranscations(PharmacyTranscations transcation)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            var query = "INSERT INTO pharmacy_transcations (total_amount, receipt_path) VALUES (@totalAmount, @receiptPath)";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@totalAmount", transcation.TotalAmount);
            cmd.Parameters.AddWithValue("@receiptPath", transcation.ReceiptPath.Trim());

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("PharmacyTranscationsAdded", transcation);
                return Ok(transcation);
            }
            return StatusCode(500, "Failed to add pharmacy transcation.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}