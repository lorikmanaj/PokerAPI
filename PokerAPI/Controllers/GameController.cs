using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PokerLogic.Models.Generic;
using PokerAPI.Hubs;
using DataService;
using Microsoft.EntityFrameworkCore;

namespace PokerAPI.Controllers
{

    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<RoomHub> _roomHub;
        private readonly ApplicationDbContext _db;

  

        //PokerRoomSvc.OnNew
        //RoomHub.AddUserInRoom
        [HttpGet("{userId}/room/{roomId}")]
        public async Task<ActionResult> NewPlayerJoined(string userId = "CfDJ8NAyOV_2snBFlaMFMZT5Q2Oiz3o43SmacVFVbk4gO0dTXd5io9-gHGx6LgShzlTYdaPbdNU6x-tbzu-mWIbm6KUbiYuPSrdPUkfUNuxHgTliLI0SDPWJJHpTSLPpbSc2-vzL-mO2Mrz0eq8hADCOsBn6N8D-euC6VYsPw4FFr8qE", int roomId = 1)
        {

            // Should implemented
            //var userToAdd = _db.ApplicationUsers.Where(_ => _.Id == userId).FirstOrDefault();

            var userToAdd = await _db.ApplicationUsers.FirstOrDefaultAsync();

            Challenge challenge = new Challenge();
            challenge.action = ChallengeActions.LOGIN;
            challenge.roomID = roomId;

            //var userData = new UserData()
            //{
            //    //Continue for the null assigned properties
            //    sessID = "ASDASd",
            //    userID = userToAdd.Id,
            //    lastChallenge = challenge,
            //    transactionID = null,
            //    status = UserDataStatus.PENDING,
            //    chips = 1000,
            //    requestForDeposit = 0
            //};

            //Added method from GameRepo (static methods)
            //GameRepo.GameRepo.AddUserInRoom(roomId, userData);


            //_gameHandler.sitFlow(1,userData);

            //var userData = _roomHub.AddUserInRoom(roomId, userId).Result;
            //Vazhdo qetu
            //var test = _roomHub.Groups.AddToGroupAsync(_roomHub.C)
            //_pokerRoomSvc.onNewPlayerSitdown(userData);

            return Ok();
        }

        //Sit in table
        //Get Room
        [HttpGet("GetRoom")]
        public async Task<ActionResult> GetRoom([FromBody] int roomId)
        {

            //GameHandler gameHandler = new GameHandler(GameRepo.GameRepo.GetAllUsersInRoom(roomId), _roomHub,_pokerRoomSvc);

            return null;
        }



        [HttpPost("SitInTable")]
        public async Task<ActionResult> SitInTable([FromBody] int roomId , int userID)
        {

            //GameHandler gameHandler = new GameHandler(GameRepo.GameRepo.GetAllUsersInRoom(roomId), _roomHub, _pokerRoomSvc);

            return null;
        }


        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        //WIHubContext<RoomHub> _roomHub;

        //public GameHandlerController(IHubContext<RoomHub> roomHub)
        //{
        //    this._roomHub = roomHub;


        //    UserData[] userDatas = new UserData[1];

        //    var userData = new UserData()
        //    {
        //        //Continue for the null assigned properties
        //        userID = "asdasdasd",
        //        sessID = "asdasdasdasd",

        //        transactionID = null,
        //        status = UserDataStatus.PENDING,
        //        chips = 123,
        //        requestForDeposit = 0
        //    };
        //    userDatas[0] = userData;
        //    GameHandler gameHandler = new GameHandler(userDatas, _roomHub);
        //    gameHandler.ingressFlow(userData);
        //}


        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
