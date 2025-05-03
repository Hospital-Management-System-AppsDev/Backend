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
                    u.id, u.name, u.email, u.username, u.password, u.sex, u.contact_number, u.birthday,
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
                        Sex = reader.GetString(12),
                        ContactNumber = reader.GetString(13),
                        Birthday = reader.GetDateTime(14),
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
                    u.id, u.name, u.email, u.username, u.password, u.sex, u.contact_number, u.birthday,
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
                        Sex = reader.GetString(12),
                        ContactNumber = reader.GetString(13),
                        Birthday = reader.GetDateTime(14),
                        specialization = reader.GetString(15),
                        is_available = reader.GetInt32(16)
                    },
                    AppointmentType = reader.GetString(4),
                    Status = reader.GetInt32(5),
                    AppointmentDateTime = reader.GetDateTime(6)
                };
                return Ok(appointment);
            }
            return NotFound($"Appointment with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving appointments: " + ex.Message);
        }
    }

    [HttpGet("by-doctor/{id}")]
    public async Task<IActionResult> GetAppointmentsByDoctor(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();
        var appointments = new List<Appointment>();

        try
        {
            string query = @"
                SELECT 
                    a.pkId, a.PatientID, a.PatientName, a.AssignedDoctor, a.AppointmentType, a.Status, a.AppointmentDateTime,
                    u.id, u.name, u.email, u.username, u.password, u.sex, u.contact_number, u.birthday,
                    d.specialization, d.is_available
                FROM appointments a
                JOIN users u ON a.AssignedDoctor = u.id AND u.role = 'doctor'
                JOIN doctors d ON u.id = d.doctor_id
                WHERE a.AssignedDoctor = @doctorId;";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@doctorId", id);
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
                        Sex = reader.GetString(12),
                        ContactNumber = reader.GetString(13),
                        Birthday = reader.GetDateTime(14),
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

    [HttpGet("available-slots/{doctorId}/{date}/{appointmentType}")]
    public async Task<IActionResult> GetDoctorAvailableSlots(int doctorId, DateTime date, string appointmentType)
    {
        using var conn = _dbConnection.GetOpenConnection();
        try
        {
            // First check if doctor exists and is available
            string doctorQuery = @"
                SELECT d.is_available 
                FROM users u
                JOIN doctors d ON u.id = d.doctor_id
                WHERE u.id = @doctorId AND u.role = 'doctor'";

            await using var doctorCmd = new MySqlCommand(doctorQuery, conn);
            doctorCmd.Parameters.AddWithValue("@doctorId", doctorId);
            var isAvailable = await doctorCmd.ExecuteScalarAsync();

            if (isAvailable == null)
            {
                return NotFound($"Doctor with ID {doctorId} not found.");
            }

            if (date.Date == DateTime.Now.Date && Convert.ToInt32(isAvailable) == 0)
            {
                return Ok(new { message = "Doctor is not available for appointments.", availableSlots = new List<DateTime>() });
            }

            // Get all appointments for this doctor on the specified date
            string appointmentsQuery = @"
                SELECT AppointmentDateTime, AppointmentType
                FROM appointments
                WHERE AssignedDoctor = @doctorId
                AND DATE(AppointmentDateTime) = DATE(@date)
                AND Status != 3"; // Exclude cancelled appointments (assuming status 3 is cancelled)

            List<(DateTime dateTime, string type)> bookedSlots = new List<(DateTime, string)>();

            await using var appCmd = new MySqlCommand(appointmentsQuery, conn);
            appCmd.Parameters.AddWithValue("@doctorId", doctorId);
            appCmd.Parameters.AddWithValue("@date", date.Date);

            await using var reader = await appCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                bookedSlots.Add((reader.GetDateTime(0), reader.GetString(1)));
            }

            // Define working hours (9 AM to 5 PM)
            DateTime startTime = date.Date.AddHours(6); // 6 AM
            DateTime endTime = date.Date.AddHours(22);  // 9 PM

            // Calculate slot duration based on appointment type
            TimeSpan slotDuration;
            switch (appointmentType.ToLower())
            {
                case "consultation":
                    slotDuration = TimeSpan.FromMinutes(30);
                    break;
                case "check up":
                    slotDuration = TimeSpan.FromHours(1);
                    break;
                case "surgery":
                    // For surgery, return only a single full-day slot if the day is completely free
                    if (!bookedSlots.Any())
                    {
                        return Ok(new
                        {
                            message = "Full day available for surgery",
                            availableSlots = new[] { startTime }
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            message = "Day already has appointments scheduled",
                            availableSlots = new List<DateTime>()
                        });
                    }
                default:
                    return BadRequest("Invalid appointment type. Valid types are: consultation, check up, surgery");
            }

            // Generate all possible slots
            List<DateTime> availableSlots = new List<DateTime>();
            DateTime currentSlot = startTime;

            while (currentSlot.Add(slotDuration) <= endTime)
            {
                bool slotAvailable = true;

                // Check if this slot overlaps with any booked appointment
                foreach (var (bookedTime, bookedType) in bookedSlots)
                {
                    TimeSpan bookedDuration;
                    switch (bookedType.ToLower())
                    {
                        case "consultation":
                            bookedDuration = TimeSpan.FromMinutes(30);
                            break;
                        case "check up":
                            bookedDuration = TimeSpan.FromHours(1);
                            break;
                        case "surgery":
                            bookedDuration = TimeSpan.FromHours(8); // Full day
                            break;
                        default:
                            bookedDuration = TimeSpan.FromHours(1); // Default
                            break;
                    }

                    // Check for overlap
                    if (currentSlot < bookedTime.Add(bookedDuration) &&
                        currentSlot.Add(slotDuration) > bookedTime)
                    {
                        slotAvailable = false;
                        break;
                    }
                }

                if (slotAvailable)
                {
                    availableSlots.Add(currentSlot);
                }

                currentSlot = currentSlot.Add(slotDuration);
            }

            return Ok(new
            {
                doctorId,
                date = date.ToString("yyyy-MM-dd"),
                appointmentType,
                slotDuration = slotDuration.ToString(),
                availableSlots
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving available slots: " + ex.Message);
        }
    }


    [HttpPost("add")]
    public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = @"
                INSERT INTO appointments 
                (PatientID, PatientName, AssignedDoctor, AppointmentType, Status, AppointmentDateTime)
                VALUES (@PatientID, @PatientName, @AssignedDoctor, @AppointmentType, @Status, @AppointmentDateTime);";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PatientID", appointment.PatientID);
            cmd.Parameters.AddWithValue("@PatientName", appointment.PatientName.Trim());
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
                    u.id, u.name, u.email, u.username, u.password, u.sex, u.contact_number, u.birthday,
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
                            Sex = reader.GetString(12),
                            ContactNumber = reader.GetString(13),
                            Birthday = reader.GetDateTime(14),
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

    [HttpPatch("update-status/{id}")]
    public async Task<IActionResult> UpdateStatusToDone(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = @"UPDATE appointments SET status = 1 WHERE pkId = @id";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("CompleteAppointment", id);
                return Ok(new { message = "Appointment marked as done successfully." });
            }

            return NotFound($"Appointment with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error updating appointment status: " + ex.Message);
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


    [HttpGet("getyears/{year}")]
    public async Task<IActionResult> GetYearsAsync(int year)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = @"SELECT COUNT(pkId) FROM appointments WHERE YEAR(AppointmentDateTime) = @year;
";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@year", year);
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return Ok(new { Year = year, NumberOfPatients = count });

        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving patient: " + ex.Message);
        }
    }

    [HttpGet("getNumAppointments/{year:int}/{month:int}")]
    public async Task<IActionResult> GetNumAppointments(int year, int month)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = @"
                SELECT MONTH(AppointmentDateTime) AS Month, COUNT(PatientID) AS NumPatients
                FROM appointments
                WHERE YEAR(AppointmentDateTime) = @year AND MONTH(AppointmentDateTime) <= @month AND status = 1
                GROUP BY MONTH(AppointmentDateTime);";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@month", month);

            var result = new int[month]; // Index 0 = Jan, Index (month-1) = the given month

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int m = Convert.ToInt32(reader["Month"]);   // e.g., 1 for Jan, 2 for Feb
                int count = Convert.ToInt32(reader["NumPatients"]);
                result[m - 1] = count; // fill result at correct month index
            }
            return Ok(result.ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving patient count: " + ex.Message);
        }
    }

    [HttpGet("by-doctor-count/{year:int}/{month:int}/{id}")]
    public async Task<IActionResult> GetAppointmentsCountByDoctor(int year, int month, int id)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = @"
                SELECT MONTH(AppointmentDateTime) AS Month, COUNT(pkId) AS NumAppointments
                FROM appointments
                WHERE YEAR(AppointmentDateTime) = @year AND MONTH(AppointmentDateTime) <= @month AND status = 1 AND AssignedDoctor = @doctorId
                GROUP BY MONTH(AppointmentDateTime);";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@month", month);
            cmd.Parameters.AddWithValue("@doctorId", id);

            var result = new int[month];

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int m = Convert.ToInt32(reader["Month"]);   // e.g., 1 for Jan, 2 for Feb
                int count = Convert.ToInt32(reader["NumAppointments"]);
                result[m - 1] = count; // fill result at correct month index
            }
            return Ok(result.ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving appointments: " + ex.Message);
        }
    }

    [HttpGet("by-type/{id}")]
    public async Task<IActionResult> GetAppointmentsByType(int id)
    {
        using var conn = _dbConnection.GetOpenConnection();

        try
        {
            string query = @"
                SELECT AppointmentType, COUNT(pkId) as TypeCount 
                FROM appointments
                WHERE AssignedDoctor = @doctorId AND status = 1
                GROUP BY AppointmentType;";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@doctorId", id);

            var result = new Dictionary<string, int>();

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string appointmentType = reader["AppointmentType"].ToString();
                int count = Convert.ToInt32(reader["TypeCount"]);
                result[appointmentType] = count;
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving appointment types count: " + ex.Message);
        }
    }
}
