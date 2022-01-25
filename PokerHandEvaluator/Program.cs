using HoldemHand;
using ModelService.GameModels;
using ModelService.RoundGameModel.Cards;
using ModelService.RoundGameModel.Deck;
using ModelService.RoundGameModel.Hands;
using PokerLogic.Models.Generic;
using System;
using System.Collections.Generic;

namespace PokerHandEvaluator
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
            handsCards.Add(new Card(1, 10));
            handsCards.Add(new Card(0, 14));

            // Hands 2
            List<Card> handsCards1 = new List<Card>();
            handsCards1.Add(new Card(1, 6));
            handsCards1.Add(new Card(1, 10));

            List<Card> tableCards = new List<Card>();
            tableCards.Add(new Card(1, 2));
            tableCards.Add(new Card(3, 10));
            tableCards.Add(new Card(1, 12));
            tableCards.Add(new Card(3, 5));
            tableCards.Add(new Card(0, 9));

            RoomModel roomModel = new RoomModel();
            roomModel.roundModel = new RoundModel();
            roomModel.roundModel.usersInGame = new UserData[]
            {
                new UserData(){userID = "1"},
                new UserData(){userID = "2"}
            };


            roomModel.roundModel.hands = new HandValues[2];
            //var getplayerHand1 = Room.GetPlayerHandRank(handsCards, tableCards);
            //var getplayerHand2 = Room.GetPlayerHandRank(handsCards1, tableCards);


            string board = "2h 5d 8c 3c 2d";
            //Hand player1 = new Hand("ac as", board);
            //Hand player2 = new Hand("ad ks", board);

            var p1 = converCardToString(handsCards);
            //string board = converCardToString(tableCards);
            Hand player1 = new Hand(p1, board);
            Hand player2 = new Hand(converCardToString(handsCards1), board);
            var sdd = Hand.MaskToString(player2.BoardMask);
            Hand hand = new Hand();
            //Hand.EvaluateType()
            var asd = GetBestFiveCard(board, player1.PocketCards, player1.MaskValue);
            var asd1 = GetBestFiveCard(board, converCardToString(handsCards1), player2.MaskValue);
            //hand.MaskValue

            // Print out a hand description: in this case both hands are
            // A straight, six high
            Console.WriteLine("Player1 Hand: {0}", player1.Description);
            Console.WriteLine("Player2 Hand: {0}", player2.Description);

            // Compare hands
            if (player1 == player2)
            {
                Console.WriteLine("player1's hand is" +
                                  " equal to player2's hand");
            }
            else if (player1 > player2)
            {
                Console.WriteLine("player1's hand is " +
                                  "greater than player2's hand");
            }
            else
            {
                Console.WriteLine("player2's hand is greater" +
                                  " than or equal player1's hand");
            }



            //roomModel.roundModel.hands[0] = new HandValues { handName = };
            roomModel.roundModel.hands[0] = GetHandValues(tableCards, handsCards);
            roomModel.roundModel.hands[1] = GetHandValues(tableCards, handsCards1);
            //HandValues handValues = desk.getHandData(handsCards, tableCards);
            //OLD

            //roomModel.roundModel.hands[0] = desk.getHandData(handsCards, tableCards);
            //roomModel.roundModel.hands[1] = desk.getHandData(handsCards1, tableCards);
            var pots = new Pot { playersForPot = new List<int> { 0, 1 }, pot = 100 };

            Room room = new Room();
            //var handValues2 = Room.GetPlayerHandRank(handsCards, tableCards);
            //Console.Write(handValues.handName);

            var winner = getWinnerOf(pots, 0, roomModel);

            //winner.ForEach(winner =>
            //{
            //    if(winner.)
            //});
            // Timer


            //var asd = Hand.MaskToString(bestmask);
        }



        private static ulong BestFiveCards(ulong hand)
        {

            uint hv = Hand.Evaluate(hand);

            // Loop through possible 5 card hands
            foreach (ulong hand5 in Hand.Hands(0UL, ~hand, 5))
            {
                if (Hand.Evaluate(hand5) == hv)
                    return hand5;
            }

            throw new Exception("Error");
        }
        static string GetBestFiveCard(string board, string winner_hand, ulong mask)
        {
            ulong totalmask = Hand.ParseHand(winner_hand + " " + board);
            ulong bestfivemask = BestFiveCards(totalmask);
            var bestfiveString = Hand.MaskToString(bestfivemask);
            var asd = converStringToCard(bestfiveString);
            return Hand.MaskToString(bestfivemask);
        }

        public static List<Card> converStringToCard(string best5cards)
        {
            var listTEmp = new List<Card>();
            string vs = "";
            string[] stringArrray = best5cards.Split(new char[0]);
            foreach (var item in stringArrray)
            {

               var cardts = new Card(converStringSuitToEnum(item[1].ToString()), convertStringtoValue(item[0].ToString()));
                listTEmp.Add(cardts);
            }
            return listTEmp;
        }

        public static int convertStringtoValue(string number)
        {
            int numberTemp = 0;
            try
            {
                numberTemp = int.Parse(number);
            }
            catch (Exception ex)
            {
                switch (number)
                {
                    case "T": numberTemp = 10; break;
                    case "J": numberTemp = 11; break;
                    case "Q": numberTemp = 12; break;
                    case "K": numberTemp = 13; break;
                    case "A": numberTemp = 14; break;
                }
            }
        
            return numberTemp;
        }

        public static int converStringSuitToEnum(string suit)
        {
            var suitString = 0;

            switch (suit)
            {
                case "h": suitString = 0; break;
                case "d": suitString = 1; break;
                case "c": suitString = 2; break;
                case "s": suitString = 3; break;
            }
            return suitString;
        }


        public static string converEnumToString(Suit suit)
        {
            var suitString = "";

            switch (suit.ordinal())
            {
                case 0: suitString = "h"; break;
                case 1: suitString = "d"; break;
                case 2: suitString = "c"; break;
                case 3: suitString = "s"; break;
            }
            return suitString;
        }

        public static string converCardToString(List<Card> cards)
        {
            string vs = "";
            for (int i = 0; i < cards.Count; i++)
            {
                if (i == cards.Count - 1)
                {
                    vs = vs + getNameOf(cards[i].value.NumericValue) + converEnumToString(cards[i].suit);
                }
                else
                {
                    vs = vs + getNameOf(cards[i].value.NumericValue) + converEnumToString(cards[i].suit) + " ";
                }


            }
            return vs;
        }
        public static String getNameOf(int number)
        {
            if (number < 11)
            {
                return "" + number;
            }
            else if (number == 11)
            {
                return "j";
            }
            else if (number == 12)
            {
                return "q";
            }
            else if (number == 13)
            {
                return "k";
            }
            return "a";
        }

        public static HandValues GetHandValues(List<Card> tableCards, List<Card> handCards)
        {
            var tableCardsTemp = converCardToString(tableCards);
            Hand player1 = new Hand(converCardToString(handCards), tableCardsTemp);
            return new HandValues { handName = player1.Description, handPoints = player1.HandValue };
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
                if (TimerIn <= 20)
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
            uint maxPoints = 0;
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
                 
                    winnersPositions.Add(winData);
                   
                }
            }
      
            return winnersPositions;
        }

        static double JacksOrBetterWinnings(uint handval, int coins)
        {
            switch ((Hand.HandTypes)Hand.HandType(handval))
            {
                case Hand.HandTypes.StraightFlush:
                    if (Hand.CardRank((int)Hand.TopCard(handval)) == Hand.RankAce)
                        if (coins < 5) return 250.0 * coins;
                        else return 4000.0;
                    return 40.0 * coins;
                case Hand.HandTypes.FourOfAKind: return 20.0 * coins;
                case Hand.HandTypes.FullHouse: return 9.0 * coins;
                case Hand.HandTypes.Flush: return 6.0 * coins;
                case Hand.HandTypes.Straight: return 4.0 * coins;
                case Hand.HandTypes.Trips: return 3.0 * coins;
                case Hand.HandTypes.TwoPair: return 2.0 * coins;
                case Hand.HandTypes.Pair:
                    if (Hand.CardRank((int)Hand.TopCard(handval)) >= Hand.RankJack)
                        return 1.0 * coins;
                    break;
            }
            return 0.0;
        }

        // Calculate the expected value with the specified holdmask
        static double ExpectedValue(uint holdmask, ref int[] cards, int bet)
        {
            ulong handmask = 0UL, deadcards = 0UL;
            double winnings = 0.0;
            long count = 0;

            // Create Hold mask and Dead card mask
            for (int i = 0; i < 5; i++)
            {
                if ((holdmask & (1UL << i)) != 0)
                    handmask |= (1UL << cards[i]);
                else
                    deadcards |= (1UL << cards[i]);
            }

            // Iterate through all possible masks
            foreach (ulong mask in Hand.Hands(handmask, deadcards, 5))
            {
                winnings += JacksOrBetterWinnings(Hand.Evaluate(mask, 5), bet);
                count++;
            }

            return (count > 0 ? winnings / count : 0.0);
        }

    }


}
