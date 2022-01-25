using ModelService.GameModels;
using ModelService.RoundGameModel.Cards;
using PokerLogic.Models.Generic;
using PokerLogic.Models.SchemaModels.inGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerLogic.Utils
{
    public class Utils
    {
        public static int checkPlayers(UserData[] usersInTable)
        {
            int countUsers = 0;
            for (int i = 0; i < usersInTable.Length; i++)
                if (usersInTable[i] != null)
                    countUsers++;

            return countUsers;
        }

        public static int getRandomPositionOfTakens(UserData[] usersInTable)
        {
            //usersInTable = usersInTable.Where(_ => _.challengeAction == ChallengeActions.DEPOSIT).ToList();
            List<int> takenSpaces = getPlayers(usersInTable);
            Random rand = new Random();
            return takenSpaces[rand.Next(takenSpaces.Count())];
        }

        public static int getNextPositionOfPlayers(UserData[] usersInGame, int actualPosition)
        {
            //usersInGame = usersInGame.Where(_ => _.challengeAction == ChallengeActions.DEPOSIT).ToList();
            // TODO: ignore players in all in

            for (int i = actualPosition + 1; i < usersInGame.Length; i++)
                if (usersInGame[i] != null)
                    return i;

            for (int i = 0; i < actualPosition; i++)
                if (usersInGame[i] != null)
                    return i;

            return actualPosition; // wtf?, this only can be if the room has configured the minimum on 1 player.
        }

        public static int countUsersCanPlay(UserData[] usersInTable)
        {
            int quantity = 0;
            for (int i = 0; i < usersInTable.Length; i++)
                if (usersInTable[i] != null && usersInTable[i].chips > 0)
                    quantity++;

            return quantity;
        }

        public static int countUsersInGame(UserData[] usersInGame)
        {
            int quantity = 0;
            for (int i = 0; i < usersInGame.Length; i++)
                if (usersInGame[i] != null)
                    quantity++;

            return quantity;
        }

        public static List<UserData> getNewArrayOfUsers(UserData[] usersInTable)
        {
            List<UserData> usersInGame = new List<UserData>();
            for (int i = 0; i < usersInTable.Length; i++)
                if (usersInTable[i] != null && usersInTable[i].chips > 0)
                    usersInGame[i] = usersInTable[i];
                else
                    usersInGame[i] = null;

            return usersInGame;
        }

        public static List<UserData> getPlayersWithoutChips(UserData[] usersInTable)
        {
            List<UserData> outt = new List<UserData>();
            foreach (UserData ud in usersInTable)
                if (ud != null && ud.chips <= 0)
                    outt.Add(ud);

            return outt;
        }

        public static List<int> getPlayers(UserData[] usersInTable)
        {
            List<int> players = new List<int>();
            for (int i = 0; i < usersInTable.Length; i++)
                if (usersInTable[i] != null)
                    players.Add(i);

            return players;
        }

        public static List<int> getPlayersOrderedByBets(double[] bets)
        {
            List<int> outt = new List<int>();
            foreach (var bet in bets)
            {
                int maxPos = -1;
                double maxBet = double.MaxValue;
                for (int i = 0; i < bets.Length; i++)
                {
                    if (bets[i] > 0)
                        if (bets[i] < maxBet && !outt.Contains(i))
                        {
                            maxPos = i;
                            maxBet = bets[i];
                        }

                }
                if (maxPos >= 0)
                    outt.Add(maxPos);
            }
            
            return outt;



  
        }

	

        public static long getMinPotAmountGreaterThanZero(long[] bets)
        {
            return bets.ToList().Where(_ => _ > 0).Min();
        }

        public static List<int> getPlayersFromPosition(UserData[] usersInTable, int startPosition)
        {
            List<int> players = new List<int>();

            for (int i = startPosition + 1; i < usersInTable.Length; i++)
                if (usersInTable[i] != null)
                    players.Add(i);

            for (int i = 0; i <= startPosition; i++)
                if (usersInTable[i] != null)
                    players.Add(i);

            return players;
        }

        public static int getPlyerPosition(UserData[] usersInGame, UserData userSearched)
        {
            // TODO: only for ready users?
            for (int i = 0; i < usersInGame.Length; i++)
                if(usersInGame[i] != null && userSearched != null)
                if (usersInGame[i].dataBlock.PlayerId == userSearched.dataBlock.PlayerId)
                    return i;

            return -1; // TODO: throw user not found.-
        }
            
        public static UserData getPlayerById(RoomModel room, int userId)
        {
            foreach (var item in room.PlayerArray)
                if (item != null)
                    if(userId == item.dataBlock.PlayerId)
                        return item;
            
            return null;
        }

        public static SchemaCard getSchemaFromCard(Card card)
        {
            return new SchemaCard(card.suit.ordinal(), card.value.NumericValue);
        }

        public static List<double> getPotValues(List<Pot> pots)
        {
            List<double> vPots = new List<double>();
            pots.ForEach(pot =>
            {
                vPots.Add(pot.pot);
            });
            return vPots;
        }

        public static bool isPlayerExistsInTablePosition(int position, RoomModel room,UserData userData) {
            bool IsPlayerExistInThisRoom = false;

            foreach (var item in room.PlayerArray)
                if (item != null && userData != null)
                    if (userData.dataBlock.PlayerId == item.dataBlock.PlayerId)
                    {
                        IsPlayerExistInThisRoom = true;
                        return IsPlayerExistInThisRoom;
                    }

            return IsPlayerExistInThisRoom;
        }

        public static void changeSessIDInUserInGamer(ref UserData[] userInGame, UserData userData)
        {
            for (int i = 0; i < userInGame.Length; i++)
                if (userInGame[i] != null && userInGame[i].dataBlock.PlayerId == userData.dataBlock.PlayerId)
                    userInGame[i].sessID = userData.sessID;
                   
        }
    }
}