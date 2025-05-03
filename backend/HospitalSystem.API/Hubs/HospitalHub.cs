using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using HospitalApp.Models;

public class HospitalHub : Hub
{
    // Doctor methods
    public async Task UpdateDoctorAvailabilityAsync(int doctorId, int isAvailable)
    {
        await Clients.All.SendAsync("UpdateDoctorAvailabilityAsync", doctorId, isAvailable);
    }

    public async Task AddDoctor(Doctor doctor)
    {
        await Clients.All.SendAsync("DoctorAdded", doctor);
    }

    // Appointment methods
    public async Task UpdateAppointment(int appointmentId, Appointment appointment)
    {
        await Clients.All.SendAsync("UpdateAppointment", appointmentId, appointment);
    }

    public async Task AddAppointment(Appointment appointment)
    {
        await Clients.All.SendAsync("AddAppointment", appointment);
    }

    public async Task CancelAppointment(int appointmentId)
    {
        await Clients.All.SendAsync("CancelAppointment", appointmentId);
    }

    public async Task CompleteAppointment(int appointmentId)
    {
        await Clients.All.SendAsync("CompleteAppointment", appointmentId);
    }

    public async Task AddRecord(Records record)
    {
        await Clients.All.SendAsync("RecordAdded", record);
    }

    public async Task AddMedicine(Medicines medicine)
    {
        await Clients.All.SendAsync("MedicineAdded", medicine);
    }

    public async Task UpdateMedicine(Medicines medicine)
    {
        await Clients.All.SendAsync("MedicineUpdated", medicine);
    }

    public async Task DeleteMedicine(int id)
    {
        await Clients.All.SendAsync("MedicineDeleted", id);
    }
}
