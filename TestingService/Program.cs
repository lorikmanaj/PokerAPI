using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using ModelService.GameModels;
using ModelService.RoundGameModel.Cards;
using ModelService.RoundGameModel.Deck;
using ModelService.RoundGameModel.Hands;
using PokerHandEvaluator;
using PokerLogic.Models.Generic;

namespace TestingService
{
    class Program
    {
        static void Main(string[] args)
        {

            //public static readonly Suit HEARTH = new Suit("HEARTH", InnerEnum.HEARTH); // corazones
            //public static readonly Suit DIAMOND = new Suit("DIAMOND", InnerEnum.DIAMOND); // diamantes
            //public static readonly Suit CLUB = new Suit("CLUB", InnerEnum.CLUB); // treboles
            //public static readonly Suit SPADE = new Suit("SPADE", InnerEnum.SPADE);

            //GameHandler gameHandler = new GameHandler(userDatas);
            //gameHandler.ingressFlow(userData); 
            //Console.WriteLine("Hello World!");

            Deck desk = new Deck();
            // Hands 1
            List<Card> handsCards = new List<Card>();
            handsCards.Add(new Card(1, 4));
            handsCards.Add(new Card(0, 3));

            // Hands 2
            List<Card> handsCards1 = new List<Card>();
            handsCards1.Add(new Card(1, 6));
            handsCards1.Add(new Card(0, 10));


            List<Card> tableCards = new List<Card>();
            tableCards.Add(new Card(1, 12));
            tableCards.Add(new Card(1, 5));
            tableCards.Add(new Card(1, 3));
            tableCards.Add(new Card(1, 11));
            tableCards.Add(new Card(0, 9));




            RoomModel roomModel = new RoomModel();
            roomModel.roundModel = new RoundModel();
            roomModel.roundModel.usersInGame = new UserData[]
            {
                new UserData(){userID = "1"},
                new UserData(){userID = "2"}
            };

            roomModel.roundModel.hands = new HandValues[2];
            var getplayerHand1 = Room.GetPlayerHandRank(handsCards, tableCards);
            var getplayerHand2 = Room.GetPlayerHandRank(handsCards1, tableCards);

            roomModel.roundModel.hands[0] = getplayerHand1;
            roomModel.roundModel.hands[1] = getplayerHand2;
            //HandValues handValues = desk.getHandData(handsCards, tableCards);
            //OLD

            //roomModel.roundModel.hands[0] = desk.getHandData(handsCards, tableCards);
            //roomModel.roundModel.hands[1] = desk.getHandData(handsCards1, tableCards);
            //var pots = new Pot { playersForPot = new List<int> { 0, 1 }, pot = 100 };




            Room room = new Room();
            var handValues2 = Room.GetPlayerHandRank(handsCards, tableCards);
            //Console.Write(handValues.handName);

            //var winner = getWinnerOf(pots, 0, roomModel);

            //winner.ForEach(winner =>
            //{
            //    if(winner.)
            //});
            // Timer


            SetTimer();

            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.WriteLine("The application started at {0:HH:mm:ss.fff}", DateTime.Now);
            Console.ReadLine();
            //aTimer.Stop();
            //aTimer.Dispose();
            TimerIn = 0;
            Console.ReadLine();
            //Console.WriteLine("Terminating the application...");


        }

        private static System.Timers.Timer aTimer;
        private static int TimerIn = 0;
        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += ((o, e) =>
            {
                TimerIn += 1;
                if(TimerIn <= 20)
                {
                    Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff} , Timer: " + TimerIn,
                e.SignalTime);
                }
                else
                {
                    aTimer.Stop();
                    TimerIn = 0;
                    aTimer.Start();
          
                }


            });

        }

      
        public static List<Winner> getWinnerOf(Pot pot, int iteration, RoomModel room)
        {
            List<Winner> winnersPositions = new List<Winner>();
            // prev winner for this pot:
            // TODO: IMPROVE prevenir recalcular revisando si los ganadores previos también son ganadores de este
            //
            int maxPoints = 0;
            int secondaryMaxPoints = 0;
            HandType handWinner = default;
            // traemos los ganadores por jerarquía
            foreach (int player in pot.playersForPot)
            {
                if (room.roundModel.hands[player] == null)
                {
                    continue;
                }
                if (room.roundModel.hands[player].handPoints > maxPoints)
                {
                    Winner winData = new Winner();
                    winnersPositions = new List<Winner>();
                    winData.fullPot = pot.pot;
                    winData.points = room.roundModel.hands[player].handPoints;
                    winData.position = player;
                    winData.reason = room.roundModel.hands[player].handName;
                    winData.secondaryPoints = room.roundModel.hands[player].secondaryHandPoint;
                    winData.potNumber = iteration;
                    maxPoints = room.roundModel.hands[player].handPoints;
                    secondaryMaxPoints = room.roundModel.hands[player].secondaryHandPoint;
                    handWinner = room.roundModel.hands[player].type;
                    winnersPositions.Add(winData);
                }
                else if (room.roundModel.hands[player].handPoints == maxPoints)
                {
                    Winner winData = new Winner();
                    winData.fullPot = pot.pot;
                    winData.points = room.roundModel.hands[player].handPoints;
                    winData.secondaryPoints = room.roundModel.hands[player].secondaryHandPoint;
                    winData.position = player;
                    winData.potNumber = iteration;
                    winData.reason = room.roundModel.hands[player].handName;
                    // juegos con doble handPoint como full o par doble que tienen puntos secundarios:
                    if (handWinner == HandType.FULL_HOUSE || handWinner == HandType.TWO_PAIRS)
                    {
                        // validamos que el secondary points sea iguales también
                        if (room.roundModel.hands[player].secondaryHandPoint > secondaryMaxPoints)
                        {
                            winnersPositions = new List<Winner>();
                            winnersPositions.Add(winData);
                            secondaryMaxPoints = room.roundModel.hands[player].secondaryHandPoint;
                        }
                        else if (room.roundModel.hands[player].secondaryHandPoint == secondaryMaxPoints)
                        {
                            winnersPositions.Add(winData);
                        }
                    }
                    else
                    {
                        winnersPositions.Add(winData);
                    }
                }
            }
            // check kickers:
            // juegos que tienen menos de 5 cartas para contar kicker:
            int bigKicker = 0;
            int smallKicker = 0;
            int thirdKicker = 0;
            int fourthKicker = 0;
            int fifthKicker = 0;
            List<Winner> cleanWinnersPositions = new List<Winner>();
            if (winnersPositions.Count > 1 && handWinner != HandType.FULL_HOUSE && handWinner != HandType.FLUSH && handWinner != HandType.STRAIGHT && handWinner != HandType.STRAIGHT_FLUSH)
            {
                foreach (var winner in winnersPositions)
                {
                    // no tengo kicker:
                    if (room.roundModel.hands[winner.position].kickerPoint.Count == 0)
                    {
                        // el kicker no existe
                        if (bigKicker == 0)
                        {
                            bigKicker = 0;
                            smallKicker = 0;
                            cleanWinnersPositions.Add(winner);
                        }
                    }
                    else
                    {
                        // tengo kicker
                        // mi kicker es mejor que el del otro
                        if (room.roundModel.hands[winner.position].kickerPoint[0] > bigKicker)
                        {
                            cleanWinnersPositions = new List<Winner>();
                            bigKicker = room.roundModel.hands[winner.position].kickerPoint[0];
                            smallKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[1] : 0;
                            if (room.roundModel.hands[winner.position].kickerPoint.Count > 2)
                                thirdKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[2] : 0;
                            if (room.roundModel.hands[winner.position].kickerPoint.Count > 3)
                                fourthKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[3] : 0;
                            if (room.roundModel.hands[winner.position].kickerPoint.Count > 4)
                                fifthKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[4] : 0;
                            cleanWinnersPositions.Add(winner);
                            // mi kicker es igual al del otro
                        }
                        else if (room.roundModel.hands[winner.position].kickerPoint[0] == bigKicker)
                        {
                            // no tengo segundo kicker:
                            if (room.roundModel.hands[winner.position].kickerPoint.Count < 2)
                            {
                                // el otro tampoco tiene segundo kicker:
                                if (smallKicker == 0)
                                {
                                    bigKicker = 0;
                                    smallKicker = 0;
                                    cleanWinnersPositions.Add(winner);
                                }
                            }
                            else
                            { // tengo segundo kicker
                              // mi segundo kicker es más grande que el del otro:
                                if (room.roundModel.hands[winner.position].kickerPoint[1] > smallKicker)
                                {
                                    cleanWinnersPositions = new List<Winner>();
                                    bigKicker = room.roundModel.hands[winner.position].kickerPoint[0];
                                    smallKicker = room.roundModel.hands[winner.position].kickerPoint[1];
                                    cleanWinnersPositions.Add(winner);
                                }
                                else if (room.roundModel.hands[winner.position].kickerPoint[1] == smallKicker)
                                { // mi segundo kicker es igual al del otro
                                    if (room.roundModel.hands[winner.position].kickerPoint.Count > 2)
                                    {
                                        if (room.roundModel.hands[winner.position].kickerPoint[2] > thirdKicker)
                                        {
                                            cleanWinnersPositions = new List<Winner>();
                                            bigKicker = room.roundModel.hands[winner.position].kickerPoint[0];
                                            smallKicker = room.roundModel.hands[winner.position].kickerPoint[1];
                                            if (room.roundModel.hands[winner.position].kickerPoint.Count > 2)
                                                thirdKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[2] : 0;
                                            if (room.roundModel.hands[winner.position].kickerPoint.Count > 3)
                                                fourthKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[3] : 0;
                                            if (room.roundModel.hands[winner.position].kickerPoint.Count > 4)
                                                fifthKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[4] : 0;

                                            cleanWinnersPositions.Add(winner);
                                        }
                                        else if (room.roundModel.hands[winner.position].kickerPoint.Count > 3)
                                        {

                                            if (room.roundModel.hands[winner.position].kickerPoint[3] > fourthKicker)
                                            {
                                                cleanWinnersPositions = new List<Winner>();
                                                bigKicker = room.roundModel.hands[winner.position].kickerPoint[0];
                                                smallKicker = room.roundModel.hands[winner.position].kickerPoint[1];
                                                if (room.roundModel.hands[winner.position].kickerPoint.Count > 2)
                                                    thirdKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[2] : 0;
                                                if (room.roundModel.hands[winner.position].kickerPoint.Count > 3)
                                                    fourthKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[3] : 0;
                                                if (room.roundModel.hands[winner.position].kickerPoint.Count > 4)
                                                    fifthKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[4] : 0;

                                                //room.roundModel.hands[winner.position].handName += "Kicker " + fourthKicker;

                                                cleanWinnersPositions.Add(winner);
                                            }
                                            if (room.roundModel.hands[winner.position].kickerPoint.Count > 4)
                                            {

                                                if (room.roundModel.hands[winner.position].kickerPoint[4] > fifthKicker)
                                                {
                                                    cleanWinnersPositions = new List<Winner>();
                                                    bigKicker = room.roundModel.hands[winner.position].kickerPoint[0];
                                                    smallKicker = room.roundModel.hands[winner.position].kickerPoint[1];
                                                    if (room.roundModel.hands[winner.position].kickerPoint.Count > 2)
                                                        thirdKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[2] : 0;
                                                    if (room.roundModel.hands[winner.position].kickerPoint.Count > 3)
                                                        fourthKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[3] : 0;
                                                    if (room.roundModel.hands[winner.position].kickerPoint.Count > 4)
                                                        fifthKicker = room.roundModel.hands[winner.position].kickerPoint.Count > 1 ? room.roundModel.hands[winner.position].kickerPoint[4] : 0;

                                                    //room.roundModel.hands[winner.position].handName += "Kicker " + fifthKicker;

                                                    cleanWinnersPositions.Add(winner);
                                                }
                                                else if (room.roundModel.hands[winner.position].kickerPoint[4] == fifthKicker)
                                                {

                                                    cleanWinnersPositions.Add(winner);
                                                }
                                            }
                                        }
                                    }

                                    else
                                    {
                                        cleanWinnersPositions.Add(winner);
                                    }


                                }
                            }
                        }
                    }
                }
            }
            else
            {
                cleanWinnersPositions = winnersPositions;
            }

            // calcular pots
            int countWinners = cleanWinnersPositions.Count;
            cleanWinnersPositions.ForEach(winner =>
            {
                winner.pot = winner.fullPot / countWinners;
                room.roundModel.usersInGame[winner.position].chips += winner.pot;
            });
            return cleanWinnersPositions;
        }
    }
}
