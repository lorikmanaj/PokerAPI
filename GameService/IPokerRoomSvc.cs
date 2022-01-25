using Microsoft.AspNetCore.SignalR;
using PokerLogic.Models.Generic;
using PokerLogic.Models.SchemaModels;
using PokerAPI.Hubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameService
{
    public interface IPokerRoomSvc
    {
		void setUsersInTableRef(UserData[] usersInTable, IHubContext<RoomHub> _roomHub);

		void checkStartGame();

		void dumpSnapshot(String sessID, Object objectID);

		void receivedMessage(SchemaGameProto schemaGameProto, String serializedMessage, String socketSessionID);

		void onNewPlayerSitdown(UserData player);

		void onDeposit(UserData player, long chipsDeposited);

		void onUserLeave(SchemaGameProto schemaGameProto, String serializedMessage, String socketSessionID);
	}
}
