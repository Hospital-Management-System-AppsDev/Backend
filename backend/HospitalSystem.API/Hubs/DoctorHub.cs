using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class DoctorHub : Hub
{
    public async Task SendDoctorAvailabilityUpdate(int doctorId, int isAvailable)
    {
        await Clients.All.SendAsync("UpdateDoctorAvailability", doctorId, isAvailable);
    }
}
