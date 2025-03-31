// using Microsoft.AspNetCore.Mvc;
// using MySql.Data.MySqlClient;
// using System.Collections.Generic;
// using Microsoft.AspNetCore.SignalR;

// [Route("api/[controller]")]
// [ApiController]
// public class UsersController : ControllerBase{
//     private readonly IHubContext<DoctorHub> _hubContext;
//     private readonly DatabaseConnection _dbConnection;

//     public UsersController(DatabaseConnection dbConnection, IHubContext<DoctorHub> hubContext)
//     {
//         _dbConnection = dbConnection;
//         _hubContext = hubContext;
//     }

//     [HttpGet]
//     public IActionResult GetUsers(){
//         using var conn = _dbConnection.GetOpenConnection();
//         var doctors = new List<User>();

//     }
// }