using Microsoft.AspNetCore.SignalR;
using ModelService;
using ModelService.RoundGameModel.Cards;
using ModelService.RoundGameModel.Deck;
using ModelService.RoundGameModel.Hands;
using PokerLogic.Models.Generic;
using PokerLogic.Models.SchemaModels;
using PokerLogic.Models.SchemaModels.inGame;
using PokerLogic.Utils;
using PokerAPI.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace PokerLogic.Models
{
    public class RoundGame
    {

        //private static sealed Logger log = LoggerFactory.getLogger(RoundGame.class);

        //private static // sessionHandlerInt // // sessionHandler;
        private static long rounds = 0;

        private static int MIN_RAISE = 50; // TODO: adjust according to configuration.
        private static int MAX_RAISE = -1; // TODO: adjust according to configuration.
        private static int SMALL_BLIND = 25; // TODO: adjust according to configuration.
        private static int BIG_BLIND = 50; // TODO: adjust according to configuration.
        private static int BLIND_MULTIPLIER = 2; // TODO: adjust according to configuration.

        // player datas:
        private UserData[] usersInGame;
        private UserMetaData[] usersInGameDescriptor; // this used for Add info to userdata in this round.
        private long[] bets;
        private Card[] playerFirstCards;
        private Card[] playerSecondCards;
        private Card[] flop;
        private Card turn;
        private Card river;
        private HandValues[] hands;

        private int roundStep;
        private int dealerPosition;
        private int waitingActionFromPlayer; // id of player are waiting.
        private int lastActionedPosition; // for cut actions.
        private int bigBlind;
        private long lastRise;
        private List<Pot> pots = new List<Pot>();

        private Deck deck;
        private bool isWaiting = false;

        private int lastActivePositionDetected;
        private ResultSet winnersResultSet;


		
		//	private bool ignoreLastActionedPositionOnce = false;

		public RoundGame(Deck deck, UserData[] usersInGame, int dealerPosition)
        {

			
            this.usersInGame = usersInGame;
            this.usersInGameDescriptor = new UserMetaData[this.usersInGame.Length];
            for (int i = 0; i < this.usersInGame.Length; i++)
            {
                if (this.usersInGame[i] != null)
                {
                    this.usersInGameDescriptor[i] = new UserMetaData();
                }
            }
			//this.bets = ArrayUtils.toPrimitive(Collections.nCopies(usersInGame.Length, 0L).toArray(new long[0]));
			//this.bets = convertIntegers(Collections.nCopies(usersInGame.Length, 0L).toArray(new long[0]));
			this.bets = new long[usersInGame.Length];

			this.playerFirstCards = new Card[usersInGame.Length];
            this.playerSecondCards = new Card[usersInGame.Length];
            this.deck = deck;
            deck.shuffle();
            this.dealerPosition = dealerPosition;
            rounds++;
        }


		public static int[] convertIntegers(List<int> integers)
		{
			int[] ret = new int[integers.Count];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = integers[i];
			}
			return ret;
		}


		public bool start()
        {
            RoundStart roundStartSchema = new RoundStart();
            roundStartSchema.dealerPosition = this.dealerPosition;
            roundStartSchema.roundNumber = rounds;
			//sessionHandler.sendToAll("/GameController/roundStart", roundStartSchema);
			//roomHub.Clients.Group("").SendAsync("/GameController/roundStart", roundStartSchema);


			bool AllInCornerCase = requestBlind(SMALL_BLIND, BIG_BLIND); // FXIME: adjust according to configuration.
            try
            {
                dealCards();
                roundStep = 1; // pre-flop
                if (!AllInCornerCase)
                {
                    sendWaitAction();
                    return false;
                }
                else
                {
                    showOff();
                    threadWait(500);
                    return finishBets();
                }
            }
            catch (System.NullReferenceException e)
            {
				Console.WriteLine("Interrupted Exception ", e);
                // FIXME: if this explode, then the cards are never ends to dealing.
            }
            return false;
        }

        private bool requestBlind(int smallBlindSize, int bigBlindSize)
        {

            int smallBlind = Utils.Utils.getNextPositionOfPlayers(usersInGame, this.dealerPosition);
            bigBlind = Utils.Utils.getNextPositionOfPlayers(usersInGame, smallBlind);
            lastRise = bigBlindSize;
			Generic.Blind blindObject = new Generic.Blind();
            blindObject.sbPosition = smallBlind;
            blindObject.sbChips = smallBlindSize;
            blindObject.bbPosition = bigBlind;
            blindObject.bbChips = bigBlindSize;

            if (usersInGame[smallBlind].chips > smallBlindSize)
            {
                usersInGame[smallBlind].chips -= smallBlindSize;
                bets[smallBlind] = smallBlindSize;
            }
            else
            {
                // ALL IN
                bets[smallBlind] = usersInGame[smallBlind].chips;
                blindObject.sbChips = bets[smallBlind];
                usersInGameDescriptor[smallBlind].isAllIn = true;
                usersInGame[smallBlind].chips = 0;

            }

            if (usersInGame[bigBlind].chips > bigBlindSize)
            {
                usersInGame[bigBlind].chips -= bigBlindSize;
                bets[bigBlind] = bigBlindSize;
            }
            else
            {
                // ALL IN
                bets[bigBlind] = usersInGame[bigBlind].chips;
                blindObject.bbChips = bets[bigBlind];
                usersInGameDescriptor[bigBlind].isAllIn = true;
                usersInGame[bigBlind].chips = 0;

            }
			//roomHub.Clients.Group("").SendAsync("/GameController/blind", blindObject);

			//sessionHandler.sendToAll("/GameController/blind", blindObject);


			int firstWAFP = -1;
            waitingActionFromPlayer = Utils.Utils.getNextPositionOfPlayers(usersInGame, bigBlind);
            while (usersInGameDescriptor[waitingActionFromPlayer].isAllIn && waitingActionFromPlayer != firstWAFP)
            {
                if (firstWAFP == -1)
                {
                    firstWAFP = waitingActionFromPlayer;
                }
                waitingActionFromPlayer = Utils.Utils.getNextPositionOfPlayers(usersInGame, bigBlind);
            }
            if (usersInGameDescriptor[waitingActionFromPlayer].isAllIn)
            {
                // todos en allIn?
                return true;
            }
            if (((waitingActionFromPlayer == bigBlind) || (
                waitingActionFromPlayer == smallBlind &&
                blindObject.bbChips == smallBlindSize)) &&
                    isAllinAllIn())
            {
                // automaticamente cerrar el juego como si todos fueran all in
                return true;
            }

            // en caso de la ciega estar en All-In
            if (usersInGameDescriptor[bigBlind].isAllIn)
            {
                lastActionedPosition = waitingActionFromPlayer;
                //			ignoreLastActionedPositionOnce = true;
            }
            else
            {
                lastActionedPosition = bigBlind;
            }
            return false;
        }

        private void sendWaitAction()
        {
            isWaiting = true;
            ActionFor aFor = new ActionFor();
            aFor.position = waitingActionFromPlayer;
            aFor.remainingTime = 30; // TODO: adjust according to configuration.
                                     // // sessionHandler.sendToAll("/GameController/actionFor", aFor);
                                     // send wait for bet decision:
                                     // // sessionHandler.sendToSessID("GameController/betDecision", usersInGame[aFor.position].sessID, calcDecision());
                                     // TODO: implement timer for wait stop.
        }

        public BetDecision calcDecision()
        {
            // action for:
            BetDecision bd = new BetDecision();
            bd.toCall = lastRise - bets[waitingActionFromPlayer];
            bd.canCheck = bd.toCall == 0;
            bd.minRaise = MIN_RAISE;
            bd.maxRaise = MAX_RAISE;
            return bd;
        }


        private void dealCards()
        {
            IList<int> players = Utils.Utils.getPlayersFromPosition(usersInGame, this.dealerPosition);
            //deck.getNextCard(); // burn a card ?.
            // first iteration:
            int lastPosition = 1;
            try
            {
                foreach (int position in players)
                {
                    playerFirstCards[position] = deck.getNextCard();
                    CardDist cd = new CardDist();
                    cd.position = position;
                    cd.cards = new bool[] { true, false };
                    // sessionHandler.sendToAll("/GameController/cardsDist", cd); // to all
                    ICardDist icd = new ICardDist();
                    icd.position = position;
                    lastPosition = position;
                    SchemaCard stCard = Utils.Utils.getSchemaFromCard(playerFirstCards[position]);
                    icd.cards = new SchemaCard[] { stCard, null };
                    // sessionHandler.sendToSessID("GameController/cardsDist", usersInGame[position].sessID, icd); // to the player
                                                                                                                // wait a moment?
                }
                // second iteration:
                foreach (int position in players)
                {
                    playerSecondCards[position] = deck.getNextCard();
                    CardDist cd = new CardDist();
                    cd.position = position;
                    cd.cards = new bool[] { true, true };
                    // sessionHandler.sendToAll("/GameController/cardsDist", cd); // to all
                    ICardDist icd = new ICardDist();
                    icd.position = position;
                    lastPosition = position;
                    SchemaCard stCard = Utils.Utils.getSchemaFromCard(playerFirstCards[position]);
                    SchemaCard ndCard = Utils.Utils.getSchemaFromCard(playerSecondCards[position]);
                    icd.cards = new SchemaCard[] { stCard, ndCard };
                    // sessionHandler.sendToSessID("GameController/cardsDist", usersInGame[position].sessID, icd); // to the player
                                                                                                                // wait a moment?
                }
            }
            catch (System.NullReferenceException npe)
            {
                Console.WriteLine("npe: " + lastPosition, npe);
            }

        }

		// Return if the round is finished
		public bool processDecision(DecisionInform dI, UserData uD)
		{
			dI.position = Utils.Utils.getPlyerPosition(usersInGame, uD);
			isWaiting = false;
			if (dI.position.Value == waitingActionFromPlayer)
			{
				bool actionDoed = false;
				bool finishedBets = false;
				// check if zero:
				if ("raise".EqualsIgnoreCase(dI.action) && dI.ammount <= 0)
				{
					dI.action = "call";
				}
				if ("fold".EqualsIgnoreCase(dI.action))
				{
					// TODO: remove me from all pots winners.
					pots.ForEach(pot => {
						for (int i = 0; i < pot.playersForPot.Count(); i++)
						{
							if (pot.playersForPot[i] == dI.position)
							{
								pot.playersForPot.Remove(i);
							}
						}
					});
					usersInGame[dI.position.Value] = null; // fold user.
					FoldDecision fd = new FoldDecision();
					fd.position = dI.position.Value;
					// sessionHandler.sendToAll("/GameController/fold", fd);
					if (checkPlayerActives() > 1)
					{
						actionDoed = true;
					}
					else
					{
						return finishBetFullFold();
					}
				}
				if ("call".EqualsIgnoreCase(dI.action))
				{
					long realBet = lastRise - bets[dI.position.Value];
					if (usersInGame[dI.position.Value].chips >= realBet)
					{
						usersInGame[dI.position.Value].chips -= realBet;
						if (usersInGame[dI.position.Value].chips == 0)
						{
							this.usersInGameDescriptor[dI.position.Value].isAllIn = true;
						}
						actionDoed = true;
						dI.ammount = realBet; // change the ammount to real count for frontend
						bets[dI.position.Value] = lastRise;
					}
					else
					{
						// TODO: review this.
						dI.ammount = usersInGame[dI.position.Value].chips;
						this.usersInGameDescriptor[dI.position.Value].isAllIn = true;
						actionDoed = true;
						bets[dI.position.Value] += usersInGame[dI.position.Value].chips;
						usersInGame[dI.position.Value].chips = 0;
					}
				}
				if ("check".EqualsIgnoreCase(dI.action))
				{
					if (lastRise == bets[dI.position.Value])
					{
						actionDoed = true;
						//lastActionedPosition = dI.position.Value;
					}
				}
				if ("raise".EqualsIgnoreCase(dI.action))
				{
					// TODO: check maximums and minimums.
					long ammount = dI.ammount;
					long initialBet = lastRise - bets[dI.position.Value];
					lastActionedPosition = dI.position.Value;
					long totalAmmount = initialBet + ammount;
					if (usersInGame[dI.position.Value].chips >= totalAmmount)
					{
						usersInGame[dI.position.Value].chips -= totalAmmount;
						if (usersInGame[dI.position.Value].chips == 0)
						{
							this.usersInGameDescriptor[dI.position.Value].isAllIn = true;
						}
						dI.ammount = totalAmmount; // change the ammount to real count for frontend
						actionDoed = true;
						bets[dI.position.Value] += totalAmmount;
						lastRise = bets[dI.position.Value];
						bigBlind = -1;
					}
					else
					{
						// TODO: Review this.
						bets[dI.position.Value] += usersInGame[dI.position.Value].chips;
						dI.ammount = initialBet + usersInGame[dI.position.Value].chips;
						usersInGame[dI.position.Value].chips = 0;
						this.usersInGameDescriptor[dI.position.Value].isAllIn = true;
						actionDoed = true;
						lastRise = bets[dI.position.Value];
						bigBlind = -1;
					}
				}

				if (actionDoed)
				{
					int nextPosition = Utils.Utils.getNextPositionOfPlayers(usersInGame, dI.position.Value);
					if ("raise".EqualsIgnoreCase(dI.action))
					{
						finishedBets = nextPlayer(nextPosition);
					}
					else
					{
						if (isAllinAllIn())
						{
							finishedBets = true;
							showOff();
							threadWait(500); // TODO: parametize this
						}
						else
						{
							if (nextPosition == bigBlind)
							{
								finishedBets = nextPlayer(nextPosition);
							}
							else if (lastActionedPosition == nextPosition || dI.position.Value == bigBlind)
							{ // if next is last or actual is last (in bigBlind case)
								finishedBets = true;
							}
							else
							{
								finishedBets = nextPlayer(nextPosition);
							}
						}

						//					if(nextPosition == bigBlind) {
						//						finishedBets = nextPlayer(nextPosition);
						//					} else {
						//						if(isAllinAllIn()) {
						//							finishedBets = true;
						//							showOff();
						//							threadWait(500); // TODO: parametize this
						//						} else if(lastActionedPosition == nextPosition || dI.position.Value == bigBlind) { // if next is last or actual is last (in bigBlind case)
						//							finishedBets = true;
						//						} else {
						//							finishedBets = nextPlayer(nextPosition);
						//						}
						//					}

					}
					// sessionHandler.sendToAll("/GameController/decisionInform", dI);
					if (finishedBets)
					{
						return finishBets();
					}
				}
				else
				{
					// TODO: error message?
				}
			}
			return false;
		}

		private int checkPlayerActives()
		{
			int count = 0;
			for (int i = 0; i < usersInGame.Length; i++)
			{
				if (usersInGame[i] != null)
				{
					count++;
					lastActivePositionDetected = i;
				}
			}
			return count;
		}

		private bool finishBetFullFold()
		{
			// check round bets:
			long pot = 0;
			for (int i = 0; i < bets.Length; i++)
			{
				pot += bets[i];
			}
			foreach (Pot potObj in pots)
			{
				pot += potObj.pot;
			}
			ResultSet rs = new ResultSet();
			rs.winners = new List<Winner>();
			int winner = lastActivePositionDetected;
			Winner winnerData = new Winner();
			winnerData.points = 0;
			winnerData.position = winner;
			winnerData.pot = pot;
			winnerData.reason = "All other players fold";
			usersInGame[winner].chips += pot;
			rs.winners.Add(winnerData);
			// sessionHandler.sendToAll("/GameController/resultSet", rs);
			threadWait(500); // TODO: parameterize
			return true;
		}

		private bool finishBets()
		{
			bigBlind = -1;
			int nextPj = Utils.Utils.getNextPositionOfPlayers(usersInGame, this.dealerPosition);
			lastActionedPosition = nextPj;
			// resetting rises:
			lastRise = 0;
			// merge de pots:
			List<Pot> newPots = SplitAndNormalizedPots();
			if (newPots.Count > 0)
			{
				if (this.pots.Count > 0)
				{
					if (this.pots[this.pots.Count - 1].playersForPot.Count == newPots[0].playersForPot.Count)
					{
						// si son los mismos jugadores unimos los pozos.
						this.pots[this.pots.Count - 1].pot += newPots[0].pot;
						newPots.RemoveAt(0);
					}
					this.pots.AddRange(newPots);
				}
				else
				{
					this.pots.AddRange(newPots);
				}
			}
			// mandamos al front la lista de pots:
			Pots schemaPots = new Pots();
			schemaPots.pots = Utils.Utils.getPotValues(pots);
			// mandamos al front el nuevo estado de fichas
			updateChips();
			// sessionHandler.sendToAll("/GameController/pots", schemaPots);
			if (roundStep == 1)
			{
				// flop:
				roundStep = 2;
				// wait a moment?
				threadWait(500); // TODO: parameterize
				dealFlop();
				if (isAllinAllIn())
				{
					threadWait(500); // TODO: parameterize
					return finishBets();
				}
				else
				{
					nextPlayer(nextPj);
				}
			}
			else if (roundStep == 2)
			{
				// turn:
				// wait a moment?
				threadWait(500); // TODO: parameterize
				roundStep = 3;
				dealTurn();
				if (isAllinAllIn())
				{
					threadWait(500); // TODO: parameterize
					return finishBets();
				}
				else
				{
					nextPlayer(nextPj);
				}
			}
			else if (roundStep == 3)
			{
				// turn:
				// wait a moment?
				threadWait(500); // TODO: parameterize
				roundStep = 4;
				dealRiver();
				if (isAllinAllIn())
				{
					threadWait(500); // TODO: parameterize
					return finishBets();
				}
				else
				{
					nextPlayer(nextPj);
				}
			}
			else if (roundStep == 4)
			{
				Console.WriteLine("-- SHOWDOWN --");
				showOff();

				checkHands(pots);

				updateChips();

				threadWait(5500); // TODO: parametize this.
				return true;
			}
			return false;
		}

		private void showOff()
		{
			ShowOff soff = new ShowOff(usersInGame.Length);
			for (int i = 0; i < usersInGame.Length; i++)
			{
				if (usersInGame[i] != null)
				{
					soff.setCards(
							i,
							Utils.Utils.getSchemaFromCard(playerFirstCards[i]),
							Utils.Utils.getSchemaFromCard(playerSecondCards[i])
					);
				}
			}
			// sessionHandler.sendToAll("/GameController/showOff", soff);
		}

		private bool nextPlayer(int nextPosition)
		{
			if (usersInGameDescriptor[nextPosition].isAllIn)
			{
				do
				{
					nextPosition = Utils.Utils.getNextPositionOfPlayers(usersInGame, nextPosition);
				} while (usersInGameDescriptor[nextPosition].isAllIn && nextPosition != lastActionedPosition);
				if (nextPosition == lastActionedPosition)
				{
					return true; // fin de la mano
				}
				else
				{
					waitingActionFromPlayer = nextPosition;
					sendWaitAction();
				}
			}
			else
			{
				waitingActionFromPlayer = nextPosition;
				sendWaitAction();
			}
			return false;
		}

		private void dealFlop()
		{
			deck.getNextCard(); // burn a card 
			FlopBegins fb = new FlopBegins();
			SchemaCard[] schemaCards = new SchemaCard[3];
			flop = new Card[3];
			for (int i = 0; i < 3; i++)
			{
				flop[i] = deck.getNextCard();
				schemaCards[i] = Utils.Utils.getSchemaFromCard(flop[i]);
			}
			fb.cards = schemaCards;
			// flop begins:
			// sessionHandler.sendToAll("/GameController/flop", fb);
		}

		private void dealTurn()
		{
			deck.getNextCard(); // burn a card 
			turn = deck.getNextCard();
			TurnBegins tb = new TurnBegins();
			tb.card = Utils.Utils.getSchemaFromCard(turn);
			// turn begins:
			// sessionHandler.sendToAll("/GameController/turn", tb);
		}

		private void dealRiver()
		{
			deck.getNextCard(); // burn a card 
			river = deck.getNextCard();
			RiverBegins rb = new RiverBegins();
			rb.card = Utils.Utils.getSchemaFromCard(river);
			// river begins:
			// sessionHandler.sendToAll("/GameController/river", rb);
		}

		private void checkHands(List<Pot> pots)
		{
			hands = new HandValues[usersInGame.Length];
			List<Card> tableCards = new List<Card>();
			for (int i = 0; i < 3; i++)
			{
				tableCards.Add(flop[i]);
			}
			tableCards.Add(turn);
			tableCards.Add(river);
			for (int i = 0; i < usersInGame.Length; i++)
			{
				if (usersInGame[i] != null)
				{
					List<Card> hand = new List<Card>();
					hand.Add(playerFirstCards[i]);
					hand.Add(playerSecondCards[i]);
					hands[i] = deck.getHandData(hand, tableCards);
				}
			}
			//		Console.WriteLine("Hands: ", hands);
			List<Winner> winners = new List<Winner>();
			List<Winner> prevWinner = null;
			int iteration = 0;
			foreach (var pot in pots)
			{
				prevWinner = getWinnerOf(pot, prevWinner, iteration);
				iteration++;
				winners.AddRange(prevWinner);
			}
			winnersResultSet = new ResultSet();
			winnersResultSet.winners = winners;
			// sessionHandler.sendToAll("/GameController/resultSet", winnersResultSet);
		}

		public List<Winner> getWinnerOf(Pot pot, List<Winner> prevWinner, int iteration)
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
				if (hands[player] == null)
				{
					continue;
				}
				if (hands[player].handPoints > maxPoints)
				{
					Winner winData = new Winner();
					winnersPositions = new List<Winner>();
					winData.fullPot = pot.pot;
					winData.points = hands[player].handPoints;
					winData.position = player;
					winData.reason = hands[player].handName;
					winData.secondaryPoints = hands[player].secondaryHandPoint;
					winData.potNumber = iteration;
					maxPoints = hands[player].handPoints;
					secondaryMaxPoints = hands[player].secondaryHandPoint;
					handWinner = hands[player].type;
					winnersPositions.Add(winData);
				}
				else if (hands[player].handPoints == maxPoints)
				{
					Winner winData = new Winner();
					winData.fullPot = pot.pot;
					winData.points = hands[player].handPoints;
					winData.secondaryPoints = hands[player].secondaryHandPoint;
					winData.position = player;
					winData.potNumber = iteration;
					winData.reason = hands[player].handName;
					// juegos con doble handPoint como full o par doble que tienen puntos secundarios:
					if (handWinner == HandType.FULL_HOUSE || handWinner == HandType.TWO_PAIRS)
					{
						// validamos que el secondary points sea iguales también
						if (hands[player].secondaryHandPoint > secondaryMaxPoints)
						{
							winnersPositions = new List<Winner>();
							winnersPositions.Add(winData);
							secondaryMaxPoints = hands[player].secondaryHandPoint;
						}
						else if (hands[player].secondaryHandPoint == secondaryMaxPoints)
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
			List<Winner> cleanWinnersPositions = new List<Winner>();
			if (winnersPositions.Count > 1 && handWinner != HandType.FULL_HOUSE && handWinner != HandType.FLUSH && handWinner != HandType.STRAIGHT && handWinner != HandType.STRAIGHT_FLUSH)
			{
				foreach (var winner in winnersPositions)
				{
					// no tengo kicker:
					if (hands[winner.position].kickerPoint.Count == 0)
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
						if (hands[winner.position].kickerPoint[0] > bigKicker)
						{
							cleanWinnersPositions = new List<Winner>();
							bigKicker = hands[winner.position].kickerPoint[0];
							smallKicker = hands[winner.position].kickerPoint.Count > 1 ? hands[winner.position].kickerPoint[1] : 0;
							cleanWinnersPositions.Add(winner);
							// mi kicker es igual al del otro
						}
						else if (hands[winner.position].kickerPoint[0] == bigKicker)
						{
							// no tengo segundo kicker:
							if (hands[winner.position].kickerPoint.Count < 2)
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
								if (hands[winner.position].kickerPoint[1] > smallKicker)
								{
									cleanWinnersPositions = new List<Winner>();
									bigKicker = hands[winner.position].kickerPoint[0];
									smallKicker = hands[winner.position].kickerPoint[1];
									cleanWinnersPositions.Add(winner);
								}
								else if (hands[winner.position].kickerPoint[1] == smallKicker)
								{ // mi segundo kicker es igual al del otro
									cleanWinnersPositions.Add(winner);
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
			cleanWinnersPositions.ForEach(winner => {
				winner.pot = winner.fullPot / countWinners;
				usersInGame[winner.position].chips += winner.pot;
			});
			return cleanWinnersPositions;
		}

		public List<Pot> SplitAndNormalizedPots()
		{
			// separamos los pozos:
			List<Pot> pozos = new List<Pot>();
			List<int> activeUsers = Utils.Utils.getPlayersOrderedByBets(bets);
			if (activeUsers.Count() > 1)
			{
				List<int> activeUsersWithoutBigBet = Utils.Utils.getPlayersOrderedByBets(bets);
				activeUsersWithoutBigBet.Remove(activeUsersWithoutBigBet.Count() - 1);
				for (int i = 0; i <= activeUsersWithoutBigBet.Count() - 1; i++)
				{
					// restamos el bet de esta posicion a las siguientes:
					 var index = activeUsersWithoutBigBet[i];
					if (bets[index] <= 0) continue;
					Pot pozo = new Pot();
					pozo.pot = 0;
					long bet = bets[index];
					for (int z = i; z <= activeUsers.Count() - 1; z++)
					{
						 var zindex = activeUsers[z];
						bets[zindex] -= bet;
						pozo.pot += bet;
						// si foldeo no lo agregamos como jugador
						if (usersInGame[zindex] != null)
						{
							pozo.playersForPot.Add(zindex);
						}
					}
					pozos.Add(pozo);
					bool morePots = false;
					for (int z = 0; z <= activeUsersWithoutBigBet.Count() - 1; z++)
					{
						 var zindex = activeUsers[z];
						if (bets[zindex] > 0)
						{
							morePots = true;
						}
					}
					if (!morePots)
					{
						break;
					}
				}
				// devolver excedente del mas grande:
				var maxBexPosition = activeUsers[activeUsers.Count() - 1];
				var excedent = bets[maxBexPosition];
				usersInGame[maxBexPosition].chips += excedent;
			}
			// unimos pozos fantasmass
			 List<Pot> ghostPots = new List<Pot>();
			pozos.ForEach(pozo => {
				if (ghostPots.Count() > 0)
				{
					var lastPozo = ghostPots[ghostPots.Count() - 1];
					if (lastPozo.playersForPot.Count() == pozo.playersForPot.Count())
					{
						lastPozo.pot += pozo.pot;
					}
					else
					{
						ghostPots.Add(pozo);
					}
				}
				else
				{
					ghostPots.Add(pozo);
				}
			});

			return ghostPots;
		}

		//public static SessionHandlerInt getSessionHandler()
		//{
		//	return // sessionHandler;
		//}

		//public static void setSessionHandler(SessionHandlerInt // sessionHandler)
		//{
		//	RoundGame.// sessionHandler = // sessionHandler;
		//}

		public long getRounds()
		{
			return rounds;
		}

		public static void increaseBlind()
		{
			SMALL_BLIND *= BLIND_MULTIPLIER;
			BIG_BLIND *= BLIND_MULTIPLIER;
		}

		public int getDealerPosition()
		{
			return dealerPosition;
		}

		public List<Pot> getPot()
		{
			return this.pots;
		}

		public long getBetOf(int position)
		{
			return bets[position];
		}

		public int getStep()
		{
			return roundStep;
		}

		public int getWaitingActionFromPlayer()
		{
			return waitingActionFromPlayer;
		}

		public bool checkWaiting()
		{
			return isWaiting;
		}

		public Card[] getCommunityCards()
		{
			if (roundStep == 2)
			{
				return flop;
			}
			if (roundStep == 3)
			{
				//ArrayUtils.AddRange(flop, turn);
				Card[] card = new Card[flop.Length + 1];
				

				return UpdateArray(card, flop, turn);
			}
			if (roundStep == 4)
			{
				Card[] card = new Card[flop.Length + 2];

				return UpdateArray(card, flop, turn, river);
			}
			return new Card[] { };
		}

		public Card[] UpdateArray(Card[] newArr,Card[] arr,Card turn)
		{
			for (int i = 0; i < arr.Length; i++) {

				arr[i] = newArr[i];
			}
			newArr[arr.Length + 1] = turn;
			return newArr;
		}

		public Card[] UpdateArray(Card[] newArr, Card[] arr, Card turn,Card river)
		{
			for (int i = 0; i < arr.Length; i++)
			{

				arr[i] = newArr[i];
			}
			newArr[arr.Length + 1] = turn;
			newArr[arr.Length + 2] = river;
			return newArr;
		}


		public void threadWait(int time)
		{
			// FIXME: fix this:
			try
			{
				Thread.Sleep(time);
			}
			catch (Exception e)
			{
				Console.WriteLine("INTERRUPTED EXCEPTION", e);
			}
		}

		public SchemaCard[] getCards(int pos)
		{
			if (playerFirstCards[pos] == null)
			{
				return null;
			}
			return new SchemaCard[] { Utils.Utils.getSchemaFromCard(playerFirstCards[pos]), Utils.Utils.getSchemaFromCard(playerSecondCards[pos]) };
		}

		public bool haveCards(int pos)
		{
			return playerFirstCards[pos] != null;
		}

		public bool isInGame(int pos)
		{
			return usersInGame[pos] != null;
		}

		public bool isAllinAllIn()
		{
			int usersInGameNotAllIn = 0;
			int userPending = 0;
			long maxBet = 0;
			int maxBeter = 0;
			for (int i = 0; i < this.usersInGame.Length; i++)
			{
				if (this.usersInGame[i] != null && !this.usersInGameDescriptor[i].isAllIn)
				{
					usersInGameNotAllIn++;
					userPending = i;
				}
				else if (this.usersInGame[i] != null)
				{
					if (bets[i] > maxBet)
					{
						maxBet = bets[i];
						maxBeter = i;
					}
				}
			}
			if (usersInGameNotAllIn == 0)
			{
				return true;
			}
			if (usersInGameNotAllIn == 1)
			{
				return bets[userPending] >= bets[maxBeter];
			}
			return false;
		}

		public static int getBigBlind()
		{
			return BIG_BLIND;
		}

		public static int getSmallBlind()
		{
			return SMALL_BLIND;
		}

		public void resendWinners(String sessID)
		{
			// sessionHandler.sendToSessID("/GameController/resultSet", sessID, winnersResultSet);
		}

		public void updateChips()
		{
			ChipStatus cs = new ChipStatus();
			cs.status = new List<IndividualChipStatus>();
			for (int i = 0; i < usersInGame.Length; i++)
			{
				if (usersInGame[i] != null)
				{
					IndividualChipStatus ics = new IndividualChipStatus();
					ics.chips = usersInGame[i].chips;
					ics.position = i;
					cs.status.Add(ics);
				}
			}
			// sessionHandler.sendToAll("/GameController/chipStatus", cs);
		}

		//private static IHubContext<RoomHub> roomHub;

		//public static IHubContext<RoomHub> getSessionHandler()
		//{
		//	return roomHub;
		//}
		//public static void setHubHandler(IHubContext<RoomHub> roomHub)
		//{
		//	RoundGame.roomHub = roomHub;
		//}

	}
}