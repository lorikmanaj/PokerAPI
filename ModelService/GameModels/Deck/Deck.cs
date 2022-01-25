using ModelService.RoundGameModel.Cards;
using ModelService.RoundGameModel.Hands;
using PokerLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.RoundGameModel.Deck
{
    public class Deck
    {
        //private static final Logger log = LoggerFactory.getLogger(Deck.class);

        private IList<Card> deck = new List<Card>();
        private int index;

        public Deck()
        {
            this.add1Deck();
            shuffle();
        }

        private void add1Deck()
        {
            for (int suit = 0; suit < 4; suit++)
            {
                for (int cardValue = 2; cardValue <= 14; cardValue++)
                {
                    Card card = new Card(suit, cardValue);
                    deck.Add(card);
                }
            }
            Console.WriteLine("New deck created. Deck have " + deck.Count() + " cards");
        }

        public void shuffle()
        {
            deck.Shuffle();
            index = 0;
            Console.WriteLine("cards shuffled.");
        }


        public Card getNextCard()
        {
            index++; // increment index.

            return deck.ElementAt(index - 1); // get the card.
        }

        public HandValues getHandData(List<Card> handCards, List<Card> tableCards)
        {
            //// TODO: homologate this function.
            //HandValues data = new HandValues();
            //List<Card> allCardsInGame = new List<Card>();

            //allCardsInGame.AddRange(handCards);
            //allCardsInGame.AddRange(tableCards);
            //handCards.Sort(new CardComparator());
            //// sorter cards:
            //allCardsInGame.Sort(new CardComparator());
            //List<Card> unUsedCards = new List<Card>();


            //allCardsInGame.ForEach(card =>
            //{
            //    unUsedCards.Add(card);
            //});

            //Dictionary<int, List<Card>> groupedCards = new Dictionary<int, List<Card>>();
            //List<int> pairs = new List<int>();
            //List<int> trips = new List<int>();
            //List<int> quads = new List<int>();

            //int cardOfHearth = 0;
            //int cardOfDiamond = 0;
            //int cardOfClub = 0;
            //int cardOfSpades = 0;
            //int bigFlush = 0;

            //int lastValue = 1;
            //int countConsecutives = 0;
            //int bigStraight = 0;
            //bool straightFlush = true;
            //int suitStraight = -1;

            //// Ace as One //
            //List<Card> cardsToAdd = new List<Card>();

            //foreach (var card in allCardsInGame)
            //{
            //    // Is an ace
            //    if (card.value.NumericValue == 14 || false)
            //    {
            //        // ERR
            //        cardsToAdd.Add(new Card(card.suit.ordinal(), 1));
            //    }
            //}
            //allCardsInGame.AddRange(cardsToAdd);

            //foreach (Card card in allCardsInGame)
            //{

            //    // straight 
            //    if (lastValue == card.value.NumericValue - 1 && countConsecutives < 5)
            //    { //  when count is over 4
            //        if (suitStraight == -1)
            //        {
            //            suitStraight = card.suit.ordinal();
            //        }
            //        countConsecutives++;
            //        bigStraight = card.value.NumericValue;
            //        if (suitStraight != card.suit.ordinal())
            //        {
            //            straightFlush = false;
            //        }
            //    }
            //    else if (countConsecutives >= 1 && countConsecutives < 5 &&  lastValue != card.value.NumericValue)
            //    { // ignore equals or when count is over 4
            //        countConsecutives = 0;
            //        suitStraight = -1;
            //        straightFlush = true;
            //    }
            //    lastValue = card.value.NumericValue;

            //    // para todo lo demás ignoramos el small ace.
            //    if (card.value.NumericValue == 1) continue;

            //    // group cards:
            //    if (!groupedCards.ContainsKey(card.value.NumericValue))
            //    {
            //        groupedCards[card.value.NumericValue] = new List<Card>();
            //    }
            //    List<Card> cardsGrouped = groupedCards[card.value.NumericValue];
            //    cardsGrouped.Add(card);
            //    if (cardsGrouped.Count() == 4)
            //    {
            //        quads.Add(card.value.NumericValue);
            //        trips.Remove(trips.IndexOf(card.value.NumericValue));
            //        unUsedCards.Remove(card);
            //    }
            //    else if (cardsGrouped.Count() == 3)
            //    {
            //        trips.Add(card.value.NumericValue);
            //        pairs.Remove(pairs.IndexOf(card.value.NumericValue));
            //        unUsedCards.Remove(card);
            //    }
            //    else if (cardsGrouped.Count() == 2)
            //    {
            //        pairs.Add(card.value.NumericValue);
            //        unUsedCards.Remove(cardsGrouped.ElementAt(1));
            //        unUsedCards.Remove(cardsGrouped.ElementAt(0));
            //    }
            //    // Flush?
            //    if (card.suit.Equals(Suit.CLUB))
            //    {
            //        cardOfClub++;
            //        if (cardOfClub > 4)
            //        {
            //            bigFlush = card.value.NumericValue;
            //        }
            //    }
            //    if (card.suit.Equals(Suit.DIAMOND))
            //    {
            //        cardOfDiamond++;
            //        if (cardOfDiamond > 4)
            //        {
            //            bigFlush = card.value.NumericValue;
            //        }
            //    }
            //    if (card.suit.Equals(Suit.HEARTH))
            //    {
            //        cardOfHearth++;
            //        if (cardOfHearth > 4)
            //        {
            //            bigFlush = card.value.NumericValue;
            //        }
            //    }
            //    if (card.suit.Equals(Suit.SPADE))
            //    {
            //        cardOfSpades++;
            //        if (cardOfSpades > 4)
            //        {
            //            bigFlush = card.value.NumericValue;
            //        }
            //    }
            //}

            //// search for pairs:
            //bool havePairs = pairs.Count() >= 1;
            //// search for two pair:
            //bool haveTwoPairs = pairs.Count() > 1;
            //// search for a trips (three of a kind)
            //bool haveTrips = trips.Count() > 0;
            //// search for Straight
            //bool haveStraight = countConsecutives >= 5;
            //// search flush
            //bool haveFlush = cardOfClub >= 5 || cardOfDiamond >= 5 || cardOfHearth >= 5 || cardOfSpades >= 5;
            //// search full house
            //bool haveFullHouse = havePairs && haveTrips;
            //// search for quads (four of a kind)
            //bool haveQuads = quads.Count() == 1;
            //// sarch for Straight Flush && Royal Flush
            //bool haveStraightFlush = haveStraight && straightFlush;

            //if (haveStraightFlush)
            //{
            //    // 112 + value of big card in straight
            //    data.handPoints = 112 + bigStraight;
            //    data.kickerPoint = null;
            //    data.handName = "Straight Flush To " + getNameOf(bigStraight) + " (" + data.handPoints + "/126)";
            //    data.type = HandType.STRAIGHT_FLUSH;
            //}
            //else if (haveQuads)
            //{
            //    // 98 + value of Quad
            //    int bigQuad = groupedCards[quads[0]].First().value.NumericValue;
            //    data.handPoints = 98 + bigQuad;
            //    // FIXME: Set kicker based on full table cards
            //    data.kickerPoint = this.getKickers(HandType.FOUR_QUADS, tableCards, handCards, unUsedCards);
            //    data.handName = "Four of a Kind, " + getNameOf(bigQuad) + "s (" + data.handPoints + "/126)";
            //    //			data.kickerName = "Biggest card " + getNameOf(data.kickerPoint);
            //    data.type = HandType.FOUR_QUADS;
            //}
            //else if (haveFullHouse)
            //{
            //    // 84 + value of trip.
            //    int bigFull = groupedCards[trips[trips.Count() - 1]].First().value.NumericValue;
            //    data.handPoints = 84 + bigFull;
            //    // secondary value of pair
            //    data.secondaryHandPoint = groupedCards[pairs[pairs.Count() - 1]].First().value.NumericValue;
            //    data.kickerPoint = null;
            //    data.handName = "Full House of " + getNameOf(bigFull) + "s with " + getNameOf(data.secondaryHandPoint) + "s (" + data.handPoints + "/126)";
            //    data.type = HandType.FULL_HOUSE;
            //}
            //else if (haveFlush)
            //{
            //    // 70 + value of big card in flush
            //    data.handPoints = 70 + bigFlush;
            //    data.kickerPoint = null;
            //    data.handName = "Flush with big card " + getNameOf(bigFlush) + " (" + data.handPoints + "/126)";
            //    data.type = HandType.FLUSH;
            //}
            //else if (haveStraight)
            //{
            //    // 56 + value of straight
            //    data.handPoints = 56 + bigStraight;
            //    data.kickerPoint = null;
            //    data.handName = "Straight to " + getNameOf(bigStraight) + " (" + data.handPoints + "/126)";
            //    data.type = HandType.STRAIGHT;
            //}
            //else if (haveTrips)
            //{
            //    // 42 + value of trip
            //    int bigTrip = groupedCards[trips[trips.Count() - 1]].First().value.NumericValue;
            //    data.handPoints = 42 + bigTrip;
            //    data.kickerPoint = this.getKickers(HandType.TRIPS, tableCards, handCards, unUsedCards);
            //    data.handName = "Three of a Kind, " + getNameOf(bigTrip) + " (" + data.handPoints + "/126)";
            //    //			data.kickerName = "Biggest card " + getNameOf(data.kickerPoint);
            //    data.type = HandType.TRIPS;
            //}
            //else if (haveTwoPairs)
            //{
            //    // 28 + value of big pair
            //    int bigPair = groupedCards[pairs[pairs.Count() - 1]].First().value.NumericValue;
            //    data.handPoints = 28 + bigPair;
            //    // secondary value of low pair
            //    data.secondaryHandPoint = groupedCards[pairs[pairs.Count() - 2]].First().value.NumericValue;
            //    // kicker
            //    data.kickerPoint = this.getKickers(HandType.TWO_PAIRS, tableCards, handCards, unUsedCards);
            //    data.handName = "Two Pair of " + getNameOf(bigPair) + " and " + getNameOf(data.secondaryHandPoint) + " (" + data.handPoints + "/126)";
            //    //			data.kickerName = "Biggest card " + getNameOf(data.kickerPoint);
            //    data.type = HandType.TWO_PAIRS;
            //}
            //else if (havePairs)
            //{
            //    // 14 + value of pairs
            //    int bigCard = groupedCards[pairs[0]].First().value.NumericValue;
            //    data.handPoints = 14 + bigCard;
            //    data.kickerPoint = this.getKickers(HandType.PAIRS, tableCards, handCards, unUsedCards);
            //    data.handName = "Pair of " + getNameOf(bigCard) + " (" + data.handPoints + "/126)";
            //    //			data.kickerName = "Biggest card " + getNameOf(data.kickerPoint);
            //    data.type = HandType.PAIRS;
            //}
            //else
            //{
            //    // 2-14
            //    data.handPoints = handCards.ElementAt(1).value.NumericValue;
            //    unUsedCards.Remove(handCards.ElementAt(1));
            //    data.kickerPoint = this.getHighCardKickers(allCardsInGame);
            //    data.handName = "High card " + getNameOf(data.handPoints) + " (" + data.handPoints + "/126)";
            //    //			data.kickerName = "Biggest card " + getNameOf(data.kickerPoint);
            //    data.type = HandType.BIG_CARD;
            //}
            //return data;
            return null;
        }


        public virtual List<int> getKickers(HandType type, List<Card> tableCards, List<Card> handCards, List<Card> unusedCards)
        {
            //unusedCards.Sort((x, y) =>
            //{
            //    return x.value.NumericValue + y.value.NumericValue;
            //});


            unusedCards = unusedCards.OrderByDescending(_ => _.value.NumericValue).ToList();
            handCards.Sort((x, y) =>
            {
                return x.value.NumericValue + y.value.NumericValue;
            });


            int[] excedent = new int[] { 0, 0 };
            List<int> kickers = new List<int>();

            var i = 0;
            foreach (var item in unusedCards)
            {
                if (type == HandType.FOUR_QUADS)
                {
                    if (i == 0)
                        kickers.Add(item.value.NumericValue);
                    else
                        break;
                }
                else if (type == HandType.TRIPS)
                {
                    if (i < 1)
                        kickers.Add(item.value.NumericValue);
                    else
                        break;
                }
                else if (type == HandType.TWO_PAIRS)
                {
                    if (i < 2)
                        kickers.Add(item.value.NumericValue);
                    else
                        break;
                }
                else if (type == HandType.PAIRS)
                {
                    if (i < 3)
                        kickers.Add(item.value.NumericValue);
                    else
                        break;
                }
                else if (type == HandType.BIG_CARD)
                {
                    if (i < 4)
                        kickers.Add(item.value.NumericValue);
                    else
                        break;
                }
                else
                {
                    kickers.Add(item.value.NumericValue);
                }

                i += 1;
            }



            //kickers.Add(card.value.NumericValue);

            excedent[1]++;
      
            return kickers;
        }

        public virtual List<int> getHighCardKickers(List<Card> AllCards)
        {
       
            List<int> kickers = new List<int>();

            AllCards = AllCards.OrderByDescending(_ => _.value.NumericValue).Take(5).ToList();


            AllCards.ForEach(item =>
            {
                kickers.Add(item.value.NumericValue);
            });

            return kickers;
        }


        public static String getNameOf(int number)
        {
            if (number < 11)
            {
                return "" + number;
            }
            else if (number == 11)
            {
                return "Jack";
            }
            else if (number == 12)
            {
                return "Queen";
            }
            else if (number == 13)
            {
                return "King";
            }
            return "ACE";
        }
    }
}
