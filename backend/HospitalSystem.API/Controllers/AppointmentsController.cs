using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using HospitalApp.Models;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly IHubContext<HospitalHub> _hubContext;
    private readonly DatabaseConnection _dbConnection;

    public AppointmentsController(DatabaseConnection dbConnection, IHubContext<HospitalHub> hubContext)
    {
        _dbConnection = dbConnection;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        using var conn = _dbConnection.GetOpenConnection();
        var appointments = new List<Appointment>();

        try
        {
            string query = @"
                SELECT 
                    a.pkId, a.PatientID, a.PatientName, a.AssignedDoctor, a.AppointmentType, a.Status, a.AppointmentDateTime,
                    u.id, u.name, u.email, u.username, u.password, u.gender, u.contact_number, u.age,
                    d.specialization, d.is_available
                FROM appointments a
                JOIN users u ON a.AssignedDoctor = u.id AND u.role = 'doctor'
                JOIN doctors d ON u.id = d.doctor_id;";

            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(new Appointment
                {
                    pkId = reader.GetInt32(0),
                    PatientID = reader.GetInt32(1),
                    PatientName = reader.GetString(2),
                    AssignedDoctor = new Doctor
                    {
                        Id = reader.GetInt32(7),
                        Name = reader.GetString(8),
                        Email = reader.GetString(9),
                        Username = reader.GetString(10),
                        Password = reader.GetString(11),
                        Gender = reader.GetString(12),
                        ContactNumber = reader.GetString(13),
                        Age = reader.GetInt32(14),
                        specialization = reader.GetString(15),
                        is_available = reader.GetInt32(16)
                    },
                    AppointmentType = reader.GetString(4),
                    Status = reader.GetInt32(5),
                    AppointmentDateTime = reader.GetDateTime(6)
                });
            }

            return Ok(appointments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving appointments: " + ex.Message);
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
    {
        using var conn =  _dbConnection.GetOpenConnection();

        try
        {
            string query = @"
                INSERT INTO appointments 
                (PatientID, PatientName, AssignedDoctor, AppointmentType, Status, AppointmentDateTime)
                VALUES (@PatientID, @PatientName, @AssignedDoctor, @AppointmentType, @Status, @AppointmentDateTime);";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PatientID", appointment.PatientID);
            cmd.Parameters.AddWithValue("@PatientName", appointment.PatientName);
            cmd.Parameters.AddWithValue("@AssignedDoctor", appointment.AssignedDoctor.Id);
            cmd.Parameters.AddWithValue("@AppointmentType", appointment.AppointmentType);
            cmd.Parameters.AddWithValue("@Status", appointment.Status);
            cmd.Parameters.AddWithValue("@AppointmentDateTime", appointment.AppointmentDateTime);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("AppointmentCreated", appointment);
                return Ok(new { message = "Appointment created successfully." });
            }

            return StatusCode(500, "Failed to create appointment.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error creating appointment: " + ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment newAppointment)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            // Step 1: Get the existing appointment
            Appointment existingAppointment = null;

            string getQuery = @"
                SELECT 
                    a.pkId, a.PatientID, a.PatientName, a.AssignedDoctor, a.AppointmentType, a.Status, a.AppointmentDateTime,
                    u.id, u.name, u.email, u.username, u.password, u.gender, u.contact_number, u.age,
                    d.specialization, d.is_available
                FROM appointments a
                JOIN users u ON a.AssignedDoctor = u.id AND u.role = 'doctor'
                JOIN doctors d ON u.id = d.doctor_id
                WHERE a.pkId = @id;";

            await using (var getCmd = new MySqlCommand(getQuery, conn))
            {
                getCmd.Parameters.AddWithValue("@id", id);
                await using var reader = await getCmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    existingAppointment = new Appointment
                    {
                        pkId = reader.GetInt32(0),
                        PatientID = reader.GetInt32(1),
                        PatientName = reader.GetString(2),
                        AssignedDoctor = new Doctor
                        {
                            Id = reader.GetInt32(7),
                            Name = reader.GetString(8),
                            Email = reader.GetString(9),
                            Username = reader.GetString(10),
                            Password = reader.GetString(11),
                            Gender = reader.GetString(12),
                            ContactNumber = reader.GetString(13),
                            Age = reader.GetInt32(14),
                            specialization = reader.GetString(15),
                            is_available = reader.GetInt32(16)
                        },
                        AppointmentType = reader.GetString(4),
                        Status = reader.GetInt32(5),
                        AppointmentDateTime = reader.GetDateTime(6)
                    };
                }
                else
                {
                    return NotFound($"Appointment with ID {id} not found.");
                }
            }

            // Step 2: Check for changes
            if (
                existingAppointment.PatientID == newAppointment.PatientID &&
                existingAppointment.PatientName == newAppointment.PatientName &&
                existingAppointment.AssignedDoctor.Id == newAppointment.AssignedDoctor.Id &&
                existingAppointment.AppointmentType == newAppointment.AppointmentType &&
                existingAppointment.Status == newAppointment.Status &&
                existingAppointment.AppointmentDateTime == newAppointment.AppointmentDateTime
            )
            {
                return Ok(new { message = "No changes detected." });
            }

            // Step 3: Update if there are changes
            string updateQuery = @"
                UPDATE appointments 
                SET PatientID = @pId, PatientName = @name, AssignedDoctor = @doc, 
                    AppointmentType = @type, Status = @stat, AppointmentDateTime = @dateTime 
                WHERE pkId = @id;";

            await using var updateCmd = new MySqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@pId", newAppointment.PatientID);
            updateCmd.Parameters.AddWithValue("@name", newAppointment.PatientName);
            updateCmd.Parameters.AddWithValue("@doc", newAppointment.AssignedDoctor.Id);
            updateCmd.Parameters.AddWithValue("@type", newAppointment.AppointmentType);
            updateCmd.Parameters.AddWithValue("@stat", newAppointment.Status);
            updateCmd.Parameters.AddWithValue("@dateTime", newAppointment.AppointmentDateTime);
            updateCmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("UpdateAppointment", id, newAppointment);
                return Ok(new { message = "Appointment updated successfully." });
            }

            return StatusCode(500, "Failed to update appointment.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error updating appointment: " + ex.Message);
        }
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        Appointment appointment = null;
        try
        {
            string query = @"
                SELECT 
                    a.pkId, a.PatientID, a.PatientName, a.AssignedDoctor, a.AppointmentType, a.Status, a.AppointmentDateTime,
                    u.id, u.name, u.email, u.username, u.password, u.gender, u.contact_number, u.age,
                    d.specialization, d.is_available
                FROM appointments a
                JOIN users u ON a.AssignedDoctor = u.id AND u.role = 'doctor'
                JOIN doctors d ON u.id = d.doctor_id
                WHERE a.pkId = @id;";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                appointment = new Appointment
                {
                    pkId = reader.GetInt32(0),
                    PatientID = reader.GetInt32(1),
                    PatientName = reader.GetString(2),
                    AssignedDoctor = new Doctor
                    {
                        Id = reader.GetInt32(7),
                        Name = reader.GetString(8),
                        Email = reader.GetString(9),
                        Username = reader.GetString(10),
                        Password = reader.GetString(11),
                        Gender = reader.GetString(12),
                        ContactNumber = reader.GetString(13),
                        Age = reader.GetInt32(14),
                        specialization = reader.GetString(15),
                        is_available = reader.GetInt32(16)
                    },
                    AppointmentType = reader.GetString(4),
                    Status = reader.GetInt32(5),
                    AppointmentDateTime = reader.GetDateTime(6)
                };
                return Ok(appointment);
            }
            return NotFound($"Doctor with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving appointments: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            string query = @"DELETE FROM appointments WHERE pkId=@id";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("AppointmentCancelled", id);
                return Ok(new { message = "Appointment cancelled successfully." });
            }

            return NotFound($"Appointment with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error cancelling appointment: " + ex.Message);
        }
    }

}
