
using ModelService.RoundGameModel.Cards;
using ModelService.RoundGameModel.Hands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerHandEvaluator
{
    public class Room
    {
        public static string[] suits = { "Club", "Diamond", "Heart", "Spade" };
        public string RoomName { get; set; }
        public List<string[]> CardsOnTable { get; set; }

        public ApplicationUser Chair0 { get; set; }
        public ApplicationUser Chair1 { get; set; }
        public ApplicationUser Chair2 { get; set; }
        public ApplicationUser Chair3 { get; set; }
        public ApplicationUser Chair4 { get; set; }

        public int PotOfChair0 { get; set; }
        public int PotOfChair1 { get; set; }
        public int PotOfChair2 { get; set; }
        public int PotOfChair3 { get; set; }
        public int PotOfChair4 { get; set; }


        /*
         * The problem to evaluate a sorted array and give it a "score" compatibles with change base problem and sum up result.
         * So we'll normalize each value to be between 0..12 so we could change it to base 13, becaue essentially we describes a base-13 number.
         * The reason for the normalize value is because division Sum {14 * 13^i} when i goes from 0 to 5, to normalize the result to be between [0,100]
         */
        public double EvaluateRankByHighestCards(List<string[]> cards, int excludeCardValue = -1, int excludeCardValue2 = -1, int limitCheck = 7, double normalize = 433175)
        {
            int i = 0;
            //double sum = 0;
            //int fixedSize = cards.Count() - 1;

            //for (int j = fixedSize; j >= 0; j--)
            //{
            //    int cardValue = Int32.Parse(cards[j][0]);
            //    if (cardValue == excludeCardValue || cardValue == excludeCardValue2)
            //        continue;

            //    int normalizedValue = cardValue - 2; // since CardValue is an integer between [2,14]

            //    sum += normalizedValue * Math.Pow(13, fixedSize - i);

            //    if (i == limitCheck - 1)
            //        break;

            //    i++;
            //}

            return i;
        }

        public static List<string[]> converCardToString(List<Card> cards)
        {
            List<string[]> vs = new List<string[]>();
            foreach (var item in cards)
            {
                vs.Add(new string[]{
                    item.value.NumericValue.ToString(), converEnumToString(item.suit)
                });
            }
            return vs;
        }

        public static string converEnumToString(Suit suit)
        {
            var suitString = "";

            switch (suit.ordinal())
            {
                case 0: suitString = "Heart"; break;
                case 1: suitString = "Diamond"; break;
                case 2: suitString = "Club"; break;
                case 3: suitString = "Spade"; break;
            }
            return suitString;
        }

        public static string converIntToString(int suit)
        {
            var suitString = "";

            switch (suit)
            {
                case 0: suitString = "Heart"; break;
                case 1: suitString = "Diamond"; break;
                case 2: suitString = "Club"; break;
                case 3: suitString = "Spade"; break;
            }
            return suitString;
        }

        public static int converStringToEnum(string suit)
        {
            var suitString = 0;

            switch (suit)
            {
                case "Heart": suitString = 0; break;
                case "Diamond": suitString = 1; break;
                case "Club": suitString = 2; break;
                case "Spade": suitString = 3; break;
            }
            return suitString;
        }


        public static Card converStringToCard(string[] cards)
        {
            return new Card(converStringToEnum(cards[0].ToString()), Int32.Parse(cards[0]));
        }

        //public static HandValues GetPlayerHandRank(List<Card> handCards, List<Card> tableCards)
        //{

        //    // SevenCards = UserPlayerCards Union CardsOnTable
        //    List<string[]> SevenCards = new List<string[]>();
        //    SevenCards.AddRange(converCardToString(handCards));
        //    SevenCards.AddRange(converCardToString(tableCards));

        //    List<int> kickerPoint = new List<int>();
        //    HandType dataType = new HandType();
        //    int secondaryHandPoint = 0;
        //    // We must have 7 cards, otherwise we can't determine his rank
        //    if (SevenCards.Count != 7)
        //        return null;

        //    // We'll sort the cards according to their value ( Card[0] - Value, Card[1] - Suit )
        //    // So we could go in one walk and check for many hand cases.
        //    // Values are between: 2 to 14 ( 11 - J, 12 - Q, 13 - K, 14 - A )

        //    SevenCards.Sort((x, y) =>
        //    {
        //        return int.Parse(x[0]) - int.Parse(y[0]);
        //    });

        //    //var test = SevenCards.Where(x => x[1] == "Spade").OrderBy(x => Int32.Parse(x[0]));

        //    /*
        //     * NOTE:
        //     * An ace can be the lowest card of a straight (ace, 2, 3, 4, 5)
        //     * or the highest card of a straight (ten, jack, queen, king, ace),
        //     * but a straight can't "wrap around"; a hand with queen, king, ace, 2, 3 would be worthless
        //     * (unless it's a flush).
        //     */

        //    int dupCount = 1, seqCount = 1, seqCountMax = 1;
        //    int maxCardValue = -1, dupValue = -1, seqMaxValue = -1;

        //    int currCardValue = -1, nextCardValue = -1;
        //    string currCardSuit = null, nextCardSuit = null;


        //    // In this section, we'll check for:
        //    // Highest Card, Pair, 2 Pairs, Three of a kind, Four of a kind.

        //    // Since the cards are sorted, we can find in O(1) the max value.
        //    maxCardValue = Int32.Parse(SevenCards[6][0]);

        //    /* 
        //     * There's no more than 3 series of dpulicates possible in 7 cards.
        //     * Struct: [Value: Count], for example:
        //     * [4, 3] : There are 3 cards of the value 4.
        //     */

        //    List<double[]> duplicates = new List<double[]>();

        //    // We'll skip the last card and check it seperatly after the loop for.
        //    for (int i = 0; i < 6; i++)
        //    {
        //        currCardValue = Int32.Parse(SevenCards[i][0]);
        //        currCardSuit = SevenCards[i][1];

        //        nextCardValue = Int32.Parse(SevenCards[i + 1][0]);
        //        nextCardSuit = SevenCards[i + 1][1];

        //        // Check for duplicates.
        //        if (currCardValue == nextCardValue)
        //        {
        //            dupCount++;
        //            dupValue = currCardValue;
        //        }
        //        // && currCardValue != nextCardValue since we didn't enter the if condition.
        //        else if (dupCount > 1)
        //        {
        //            duplicates.Add(new double[] { dupCount, currCardValue });
        //            dupCount = 1;
        //        }

        //        // Checks for a sequences
        //        if (currCardValue + 1 == nextCardValue)
        //        {
        //            seqCount++;
        //        }

        //        /*
        //         * Another edge case:
        //         * The reason why we are checking if currCardValue != nextCardValue, is to ensure that 
        //         * cases like these: 7,8,8,8,9,10,11 will also consider as a straight of seqCount = 5,
        //         * therefore we'll reset the seqCount if and only if currValue != nextValue completly, but if it
        //         * has the same number, we'll just simply won't seqCount++, but won't reset the counter.
        //         */

        //        // && currCardValue + 1 != nextCardValue , because we didn't enter the if condition.
        //        else if (currCardValue != nextCardValue)
        //        {
        //            if (seqCount > seqCountMax)
        //            {
        //                seqCountMax = seqCount;
        //                seqMaxValue = currCardValue;
        //            }

        //            seqCount = 1;
        //        }
        //    }

        //    // The 7th card should be checked here


        //    if (seqCount > seqCountMax)
        //    {
        //        seqCountMax = seqCount;
        //        seqMaxValue = nextCardValue;
        //    }

        //    if (dupCount > 1)
        //    {
        //        duplicates.Add(new double[] { dupCount, nextCardValue });
        //    }

        //    /* 
        //     * if we got this far it means we finished to calculate everything needed and we
        //     * are ready to start checks for the player's hand rank.
        //     */

        //    List<string[]> rankCards = new List<string[]>(); // The cards of the player
        //    string rankName = null;
        //    double rank = 0;

        //    // Checks for Royal King: rank: 900
        //    if (SevenCards[6][0] == "14" && SevenCards[5][0] == "13" && SevenCards[4][0] == "12" && SevenCards[3][0] == "11" && SevenCards[2][0] == "10"
        //        && SevenCards[6][1] == SevenCards[5][1] && SevenCards[6][1] == SevenCards[4][1] && SevenCards[6][1] == SevenCards[3][1] && SevenCards[6][1] == SevenCards[2][1])
        //    {
        //        rankName = "Royal King";
        //        rank = 127;
        //        dataType = HandType.ROYAL_FLUSH;
        //        kickerPoint = null;
        //        rankCards.AddRange(SevenCards.Skip(2));
        //    }
        //    else
        //    {

        //        // Checks for Straight Flush, rank: [800, 900)
        //        foreach (string suit in suits)
        //        {
        //            var suitCards = SevenCards.Where(x => x[1].Equals(suit)).ToList();
        //            if (suitCards.Count >= 5)
        //            {
        //                // There's no way for duplicates, since every card in the same suit is unique.
        //                int counter = 1, lastValue = -1;
        //                for (int i = 0; i < suitCards.Count - 1; i++)
        //                    if (Int32.Parse(suitCards[i][0]) + 1 == Int32.Parse(suitCards[i + 1][0]))
        //                    {
        //                        counter++;
        //                        lastValue = Int32.Parse(suitCards[i + 1][0]);
        //                        rankCards.Add(suitCards[i]);
        //                    }
        //                    else
        //                    {
        //                        counter = 1;
        //                        rankCards.Clear();
        //                    }

        //                if (counter >= 5)
        //                {
        //                    //rankName = "Straight Flush";
        //                    //rank = 800 + (double)lastValue / 14 * 99;

        //                    rank = 112 + lastValue;
        //                    rankName = "Straight Flush To " + getNameOf(lastValue) + " (" + rank + "/126)";
        //                    kickerPoint = null;
        //                    dataType = HandType.STRAIGHT_FLUSH;
        //                }

        //                // Will cover situations like this: 2,3,4,5,A,A,A
        //                // In that case we should check the 3 last cards if they are Ace and have the same suit has the 4th card.

        //                // Edge case where we have: 2,3,4,5 and then somewhere 14 ( must be last card )
        //                // In that case we'll declare as Straight Flush as well with highest card 5.
        //                else if (counter == 4 && lastValue == 5 && suitCards[suitCards.Count - 1][0] == "14")
        //                {
        //                    //rankName = "Straight Flush";
        //                    //rank = 835.3571; // The result of: 800 + 5 / 14 * 99
        //                    rank = 112 + lastValue;
        //                    rankName = "Straight Flush To " + getNameOf(lastValue) + " (" + rank + "/126)";
        //                    kickerPoint = null;
        //                    dataType = HandType.STRAIGHT_FLUSH;
        //                }
        //            }
        //        }

        //        if (rankName == null)
        //        {
        //            // For the other cases we'll sort descend the duplicates cards according by the amount.
        //            duplicates.Sort((x, y) => (int)y[0] - (int)x[0]);

        //            // Checks for Four of a kind, rank: [700, 800)
        //            if (duplicates.Count > 0 && duplicates[0][0] == 4)
        //            {
        //                rank = 98 + duplicates[0][1];
        //                rankName = "Four of a Kind, " + getNameOf(Convert.ToInt32(duplicates[0][1])) + "s (" + rank + "/126)";
        //                dataType = HandType.FOUR_QUADS;
        //                kickerPoint = new List<int> { 0 };
        //                //kickerPoint = getKickers(HandType.FOUR_QUADS, tableCards, handCards, unUsedCards);

        //                //rank = 700 + duplicates[0][1] / 14 * 50 + EvaluateRankByHighestCards(SevenCards, (int)duplicates[0][1], -1, 1);
        //                //var asd = duplicates[0][1];

        //                foreach (string suit in suits)
        //                    rankCards.Add(new string[] { duplicates[0][1].ToString(), suit });
        //            }

        //            // Checks for a Full House, rank: [600, 700)
        //            // Edge case: there are 2 pairs of 2 and one Pair of 3, for example: 33322AA
        //            else if (duplicates.Count > 2 && duplicates[0][0] == 3 && duplicates[1][0] == 2 && duplicates[2][0] == 2)
        //            {
        //                // In that edge case, we'll check from the two pairs what is greater.
        //                //rankName = "Full House";
        //                double maxTmpValue = Math.Max(duplicates[1][1], duplicates[2][1]);

        //                //rank = 600 + (duplicates[0][1]) + maxTmpValue / 14;
        //                //rank = 84 + duplicates[0][1];
        //                rank = 84 + duplicates[0][1];
        //                rankName = "Full House of " + getNameOf(Convert.ToInt32(duplicates[0][1])) + "s with " + getNameOf(Convert.ToInt32(duplicates[1][1])) + "s (" + rank + "/126)";
        //                kickerPoint = null;
        //                dataType = HandType.FULL_HOUSE;
        //                secondaryHandPoint = Convert.ToInt32(duplicates[1][1]);
        //                for (int i = 0; i < 3; i++)
        //                    rankCards.Add(new string[] { duplicates[0][1].ToString(), null });

        //                for (int i = 0; i < 2; i++)
        //                    rankCards.Add(new string[] { maxTmpValue.ToString(), null });
        //            }
        //            else if (duplicates.Count > 1 && duplicates[0][0] == 3 && duplicates[1][0] == 2)
        //            {
        //                rank = 84 + duplicates[0][1];
        //                rankName = "Full House of " + getNameOf(Convert.ToInt32(duplicates[0][1])) + "s with " + getNameOf(Convert.ToInt32(duplicates[1][1])) + "s (" + rank + "/126)";
        //                dataType = HandType.FULL_HOUSE;
        //                secondaryHandPoint = Convert.ToInt32(duplicates[1][1]);
        //                kickerPoint = null;
        //                // double[] threePairsValues = new double[] { duplicates[0][1], duplicates[1][1], duplicates[2][1] };
        //                //rank = 600 + (duplicates[0][1]) + duplicates[1][1] / 14;

        //                for (int i = 0; i < 3; i++)
        //                    rankCards.Add(new string[] { duplicates[0][1].ToString(), null });

        //                for (int i = 0; i < 2; i++)
        //                    rankCards.Add(new string[] { duplicates[1][1].ToString(), null });
        //            }

        //            // Edge case where there are 2 pairs of Three of a kind
        //            // For example if the cae is 333 222 then we'll check what is better: 333 22 or 222 33.
        //            else if (duplicates.Count > 1 && duplicates[0][0] == 3 && duplicates[1][0] == 3)
        //            {
        //                rankName = "Full House";

        //                double rank1, rank2;
        //                rank1 = 600 + (duplicates[0][1]) + duplicates[1][1] / 14;
        //                rank2 = 600 + (duplicates[1][1]) + duplicates[0][1] / 14;

        //                if (rank1 > rank2)
        //                {
        //                    rank = 84 + duplicates[0][1];
        //                    rankName = "Full House of " + getNameOf(Convert.ToInt32(duplicates[0][1])) + "s with " + getNameOf(Convert.ToInt32(duplicates[1][1])) + "s (" + rank + "/126)";
        //                    rank = rank1;
        //                    secondaryHandPoint = Convert.ToInt32(duplicates[1][1]);
        //                    for (int i = 0; i < 3; i++)
        //                        rankCards.Add(new string[] { duplicates[0][1].ToString(), null });

        //                    for (int i = 0; i < 2; i++)
        //                        rankCards.Add(new string[] { duplicates[1][1].ToString(), null });
        //                }
        //                else
        //                {
        //                    rank = 84 + duplicates[1][1];
        //                    rankName = "Full House of " + getNameOf(Convert.ToInt32(duplicates[1][1])) + "s with " + getNameOf(Convert.ToInt32(duplicates[0][1])) + "s (" + rank + "/126)";
        //                    rank = rank2;
        //                    secondaryHandPoint = Convert.ToInt32(duplicates[0][1]);
        //                    for (int i = 0; i < 3; i++)
        //                        rankCards.Add(new string[] { duplicates[1][1].ToString(), null });

        //                    for (int i = 0; i < 2; i++)
        //                        rankCards.Add(new string[] { duplicates[0][1].ToString(), null });
        //                }
        //                kickerPoint = null;
        //            }

        //            else
        //            {
        //                // Checks for Flush, rank: [500, 600)

        //                foreach (string suit in suits)
        //                {
        //                    var suitCards = SevenCards.Where(x => x[1].Equals(suit));
        //                    int suitCardsLen = suitCards.Count();
        //                    if (suitCardsLen >= 5)
        //                    {
        //                        // We only want the five last card
        //                        var suitCardsResult = suitCards.Skip(suitCardsLen - 5).ToList();

        //                        //rank = 500 + EvaluateRankByHighestCards(suitCardsResult);
        //                        var bigFlush = Convert.ToInt32(suitCardsResult.Last()[0]);
        //                        rank = 70 + bigFlush;
        //                        kickerPoint = null;
        //                        rankName = "Flush with big card " + getNameOf(bigFlush) + " (" + rank + "/126)";
        //                        rankCards.AddRange(suitCardsResult);
        //                        dataType = HandType.FLUSH;
        //                        break;
        //                    }
        //                }

        //                if (rankName == null)
        //                {
        //                    // Checks for Straight, rank: [400, 500)
        //                    if (seqCountMax >= 5)
        //                    {
        //                        rank = 56 + seqMaxValue;
        //                        kickerPoint = null;
        //                        rankName = "Straight with big card " + getNameOf(seqMaxValue) + "(" + rank + " / 126)";
        //                        dataType = HandType.FLUSH;
        //                        //rank = 400 + (double)seqMaxValue / 14 * 99;

        //                        for (int i = seqMaxValue; i > seqCountMax; i--)
        //                            rankCards.Add(new string[] { i.ToString(), null });
        //                    }

        //                    // Edge case: there's seqCountMax of 4, and the highest card is 5,
        //                    // Which means the sequence looks like this: 2, 3, 4, 5
        //                    // In that case, we'll check if the last card is Ace to complete a sequence of 5 cards.
        //                    else if (seqCountMax == 4 && seqMaxValue == 5 && maxCardValue == 14)
        //                    {
        //                        //rankName = "Straight";

        //                        //// In that case the highest card of the straight will be 5, and not Ace.
        //                        //rank = 435.3571; // The result of 400 + 5/14 * 99
        //                        kickerPoint = null;
        //                        rank = 56 + seqMaxValue;
        //                        rankName = "Straight to " + getNameOf(seqMaxValue) + "(" + rank + " / 126)";
        //                        dataType = HandType.STRAIGHT;
        //                        rankCards.Add(new string[] { "14", null });
        //                        for (int i = 2; i < 5; i++)
        //                            rankCards.Add(new string[] { i.ToString(), null });

        //                    }

        //                    // Checks for Three of a kind, rank: [300, 400)
        //                    else if (duplicates.Count > 0 && duplicates[0][0] == 3)
        //                    {
        //                        kickerPoint = new List<int> { 0 };
        //                        rank = 42 + duplicates[0][1];
        //                        rankName = "Three of a Kind, " + getNameOf(Convert.ToInt32(duplicates[0][1])) + " (" + rank + "/126)";
        //                        dataType = HandType.TRIPS;
        //                        //rank = 300 + duplicates[0][1] / 14 * 50 + EvaluateRankByHighestCards(SevenCards, (int)duplicates[0][1]);


        //                        for (int i = 0; i < 3; i++)
        //                            rankCards.Add(new string[] { duplicates[0][1].ToString(), duplicates[0][0].ToString() });

        //                        kickerPoint = getKickers(dataType, tableCards, handCards, getUnusedCards(rankCards, SevenCards));

        //                        // Edge case: there are 2 pairs of Three of a kind, in that case we'll choose the higher one.
        //                        // PROBABLY WRONG ^ because it's a full house case.
        //                        /*if (duplicates.Count > 1 && duplicates[1][0] == 3)
        //                        {
        //                            double tmpSaveMax = Math.Max(duplicates[0][1], duplicates[1][1]);

        //                            rank = 300 + tmpSaveMax / 14 * 50 + EvaluateRankByHighestCards(SevenCards, (int)tmpSaveMax);

        //                            for (int i = 0; i < 3; i++)
        //                                rankCards.Add(new string[] { tmpSaveMax.ToString(), null });
        //                        }
        //                        else
        //                        {
        //                            rank = 300 + duplicates[0][1] / 14 * 50 + EvaluateRankByHighestCards(SevenCards, (int)duplicates[0][1]);

        //                            for (int i = 0; i < 3; i++)
        //                                rankCards.Add(new string[] { duplicates[0][1].ToString(), null });
        //                        }*/
        //                    }

        //                    // Checks for Two Pairs, rank: [200, 300)
        //                    else if (duplicates.Count > 1 && duplicates[0][0] == 2 && duplicates[1][0] == 2)
        //                    {
                       
        //                        // Edge case: there are 3 pairs of Two Pairs, in that case we'll choose the higher one.
        //                        if (duplicates.Count > 2 && duplicates[2][0] == 2)
        //                        {
        //                            //rank = 200 + Math.Max(duplicates[0][1], Math.Max(duplicates[1][1], duplicates[2][1])) / 14 * 99 + (double)maxCardValue / 14;

        //                            double[] threePairsValues = new double[] { duplicates[0][1], duplicates[1][1], duplicates[2][1] };
        //                            Array.Sort(threePairsValues, (x, y) => (int)(y - x));
        //                            rank = 28 + duplicates[2][1];
        //                            kickerPoint = new List<int> { 0 };
        //                            rankName = "Three Pair of Two Pair of " + getNameOf(Convert.ToInt32(duplicates[2][1])) + " and " + getNameOf(Convert.ToInt32(duplicates[1][1])) +" and " + getNameOf(Convert.ToInt32(duplicates[0][1])) + " (" + rank + "/126";
        //                            dataType = HandType.TRIPS;
        //                            // The reason for 50 is because maxCardValue/14 can be 1, and we don't want to get the score 300.
        //                            // and its also the reason for /392 instead of /14 is.

        //                            //rank = 200 + (Math.Pow(threePairsValues[0], 2) / 392 + Math.Pow(threePairsValues[1], 2) / 392) * 50 + EvaluateRankByHighestCards(SevenCards, (int)threePairsValues[0], (int)threePairsValues[1]);


        //                            //rankName = "Two Pair of " + getNameOf(bigPair) + " and " + getNameOf(data.secondaryHandPoint) + " (" + data.handPoints + "/126)";

        //                            // We need only the 2 highest pairs from the 3 pairs.
        //                            rankCards.Add(new string[] { threePairsValues[0].ToString(), duplicates[2][0].ToString() });
        //                            rankCards.Add(new string[] { threePairsValues[1].ToString(), duplicates[1][0].ToString() });
        //                            //FIXME: ERRORRRRR NULL
        //                            kickerPoint = getKickers(dataType, tableCards, handCards, getUnusedCards(rankCards, SevenCards));
        //                        }
        //                        else
        //                        {
        //                            rank = 28 + duplicates[1][1];
        //                            kickerPoint = new List<int> { 0 };
        //                            rankName = "Two Pair of " + getNameOf(Convert.ToInt32(duplicates[1][1])) + " and " + getNameOf(Convert.ToInt32(duplicates[0][1])) + " (" + rank + "/126";
        //                            secondaryHandPoint = Convert.ToInt32(duplicates[0][1]);
        //                            dataType = HandType.TWO_PAIRS;
        //                            //rank = 200 + Math.Max(duplicates[0][1], duplicates[1][1]) / 14 * 99 + EvaluateRankByHighestCards(SevenCards, (int)duplicates[0][1], (int)duplicates[1][1]);
        //                            //rank = 200 + (Math.Pow(duplicates[0][1], 2) / 392 + Math.Pow(duplicates[1][1], 2) / 392) * 50 + EvaluateRankByHighestCards(SevenCards, (int)duplicates[0][1], (int)duplicates[1][1]);

        //                            for (int i = 0; i < 2; i++)
        //                                rankCards.Add(new string[] { duplicates[0][1].ToString(), duplicates[0][0].ToString() });

        //                            for (int i = 0; i < 2; i++)
        //                                rankCards.Add(new string[] { duplicates[1][1].ToString(), duplicates[0][0].ToString() });

        //                            kickerPoint = getKickers(dataType, tableCards, handCards, getUnusedCards(rankCards, SevenCards));
        //                        }
        //                    }

        //                    // Check for One Pair, rank: [100, 200)
        //                    else if (duplicates.Count > 0 && duplicates[0][0] == 2)
        //                    {
        //                        kickerPoint = new List<int> { 0 };
        //                        rank = 14 + duplicates[0][1];
        //                        rankName = "Pair of " + getNameOf(Convert.ToInt32(duplicates[0][1])) + " (" + rank + "/126)";
        //                        dataType = HandType.PAIRS;
        //                        //rank = 100 + duplicates[0][1] / 14 * 50 + EvaluateRankByHighestCards(SevenCards, (int)duplicates[0][1], -1, 3);

        //                        for (int i = 0; i < 2; i++)
        //                            rankCards.Add(new string[] { duplicates[0][1].ToString().ToString(),duplicates[0][0].ToString() });

        //                        kickerPoint = getKickers(dataType, tableCards, handCards, getUnusedCards(rankCards, SevenCards));
        //                    }

        //                    // Otherwise, it's High Card, rank: [0, 100)
        //                    else
        //                    {
        //                        var handPoints = Convert.ToInt32(maxCardValue.ToString());
        //                        rank =  handPoints;
        //                        kickerPoint = new List<int> { 0 };
        //                        //kickerPoint = getKickers(HandType.BIG_CARD, tableCards, handCards, SevenCards);
        //                        rankName = "High card " + getNameOf(handPoints) + " (" + rank + "/126)";
        //                        dataType = HandType.BIG_CARD;
        //                        //rankName = "High Card";
        //                        //rank = EvaluateRankByHighestCards(SevenCards, -1, -1, 5);
        //                        rankCards.Add(new string[] { maxCardValue.ToString(), null });

        //                       kickerPoint = getHighCardKickers(SevenCards);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return new HandValues
        //    {
        //        handName = rankName,
        //        handPoints = Convert.ToInt32(rank),
        //        type = dataType,
        //        secondaryHandPoint = secondaryHandPoint,
        //        kickerPoint = kickerPoint
        //    };
        //    //return new HandRank
        //    //{
        //    //    RankName = rankName,
        //    //    Rank = rank,
        //    //    Cards = rankCards
        //    //};
        //}

        public static List<Card> getUnusedCards(List<string[]> rankCards, List<string[]> SevenCards)
        {
            var cards = new List<Card>();

            rankCards.ForEach(card => {
                card[1] = converIntToString(int.Parse(card[1]));
            });

            SevenCards.ForEach(card =>
            {
                if (!rankCards.Any(_ => _[0] == card[0])){
                    cards.Add(converStringToCard(card));
                    //cards.Add(card[0][0],)
                    //cards.Add(c)
                }
            });

            return cards;
        }


        public static List<int> getKickers(HandType type, List<Card> tableCards, List<Card> handCards, List<Card> unusedCards)
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
                    if (i <= 1)
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

        public static List<int> getHighCardKickers(List<string[]> AllCards)
        {
            
            var cards = new List<Card>();
            AllCards.ForEach(card => {
                cards.Add(new Card(converStringToEnum(card[1]), int.Parse(card[0])));
            });

            List<int> kickers = new List<int>();

            cards = cards.OrderByDescending(_ => _.value.NumericValue).Take(5).ToList();


            cards.ForEach(item =>
            {
                kickers.Add(item.value.NumericValue);
            });

            return kickers;
        }

        public int getPotByIndex(int index)
        {
            return index switch
            {
                0 => PotOfChair0,
                1 => PotOfChair1,
                2 => PotOfChair2,
                3 => PotOfChair3,
                4 => PotOfChair4,
                _ => -1,
            };
        }

        public void setPotByIndex(int index, int amount)
        {
            switch (index)
            {
                case 0: PotOfChair0 = amount; break;
                case 1: PotOfChair1 = amount; break;
                case 2: PotOfChair2 = amount; break;
                case 3: PotOfChair3 = amount; break;
                case 4: PotOfChair4 = amount; break;
            }
        }

        public ApplicationUser getUserByIndex(int index)
        {
            return index switch
            {
                0 => Chair0,
                1 => Chair1,
                2 => Chair2,
                3 => Chair3,
                4 => Chair4,
                _ => null,
            };
        }

        public int getZeroPotsAmount()
        {
            int zeroCounts = 0;
            for (int i = 0; i < 5; i++)
            {
                if (getPotByIndex(i) == 0)
                    zeroCounts++;
            }

            return zeroCounts;
        }

        // Will return zero if and only if getZeroPotAmount() = 5
        public int getMinPotAmountGreaterThanZero()
        {
            int minAmount = int.MaxValue;
            for (int i = 0; i < 5; i++)
            {
                int getPot = getPotByIndex(i);
                if (minAmount > getPot && getPot > 0)
                {
                    minAmount = getPot;
                }
            }

            return minAmount;
        }

        public class SidePot
        {
            public string Name { get; set; } // for example Main Pot, Side Pot 1, Side Pot 2 , ...
            public int PotAmount { get; set; }
            public int OriginalPotAmount { get; set; }
            public List<ApplicationUser> ContestedBy { get; set; }
            public List<ApplicationUser> Winners { get; set; }
            public string RankName { get; set; } // For example: Full House, Straight , High Card , ...
            public List<string[]> WinningCards { get; set; }
        }

        public class HandRank
        {
            public string RankName { get; set; }
            public double Rank { get; set; }
            public List<string[]> Cards { get; set; }
            public ApplicationUser User { get; set; }
            public int WinningPrice { get; set; }
        }

        public class CombineHandsAndWinners
        {
            public List<List<HandRank>> totalWinners;
            public List<SidePot> potList;

            public CombineHandsAndWinners(List<List<HandRank>> totalWinners, List<SidePot> potList)
            {
                this.totalWinners = totalWinners;
                this.potList = potList;
            }
        }

        public List<SidePot> CalculateMainAndSidePots()
        {
            List<SidePot> finalPots = new List<SidePot>();

            int sidePotCounter = 0;
            var get = getZeroPotsAmount();
            while (getZeroPotsAmount() < 4)
            {
                int smallestDeck = getMinPotAmountGreaterThanZero();
                List<ApplicationUser> contestedBy = new List<ApplicationUser>();
                SidePot sidePot = new SidePot();

                // In that case, 5 is the max players occupied.
                for (int i = 0; i < 5; i++)
                {
                    if (getPotByIndex(i) >= smallestDeck)
                    {
                        // Saves the original pot of the user
                        if (sidePot.OriginalPotAmount == 0)
                            sidePot.OriginalPotAmount = getPotByIndex(i);
                        var potts = getPotByIndex(i) - smallestDeck;
                        setPotByIndex(i, potts);
                        contestedBy.Add(getUserByIndex(i));
                    }
                }

                string potName = sidePotCounter == 0 ? "Main Pot" : "Side Pot " + sidePotCounter;
                sidePot.Name = potName;
                sidePot.PotAmount = smallestDeck * contestedBy.Count;
                sidePot.ContestedBy = contestedBy;

                finalPots.Add(sidePot);

                sidePotCounter++;
            }


            // Edge case where there's still money in the room's sum pot, in that case we need to return the money to the user owned it.
            if (getZeroPotsAmount() == 4)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (getPotByIndex(i) > 0)
                    {
                        ApplicationUser user = getUserByIndex(i);
                        user.Chips += getPotByIndex(i);
                        setPotByIndex(i, 0);

                        break;
                    }
                }
            }

            return finalPots;
        }

        public List<SidePot> SpreadMoneyToWinners()
        {
            List<HandRank> handList = new List<HandRank>();
            List<SidePot> sidePotList = null;

            for (int i = 0; i < 5; i++)
            {
                ApplicationUser user = getUserByIndex(i);
                if (user != null && user.IsPlayingThisGame && getPotByIndex(i) > 0)
                {
                    //var playerRank = GetPlayerHandRank(user);
                    //if (playerRank != null)
                    //    handList.Add(playerRank);
                }
            }

            // Sort Descending by rank the handlist
            //handList.Sort((x, y) => (int)(y.Rank - x.Rank));
            handList = handList.OrderByDescending(x => x.Rank).ToList();


            //handList = handList.GroupBy(x => x.Rank).Select(grp => grp.ToList()).ToList();
            //List<List<HandRank>> totalWinners = new List<List<HandRank>>();
            List<List<HandRank>> totalWinners = handList.GroupBy(x => x.Rank).Select(grp => grp.ToList()).ToList();

            //var json = JsonConvert.SerializeObject(totalWinners);

            sidePotList = this.CalculateMainAndSidePots();


            /*
             * For each winner group, for example 1st winners, we'll go through every main/side pot
             * and we'll check if the intersection between the winner group and the contested by's pot has a match,
             * if it does, we'll split the money equally between the winners, otherwise, we'll go to the next winner group,
             * ( for example the 2nd winner groups ), it all depends on how much money every player placed in the first place.
             */

            foreach (List<HandRank> winnerGroup in totalWinners)
            {
                List<ApplicationUser> winnerUsers = new List<ApplicationUser>();
                foreach (HandRank hand in winnerGroup)
                    winnerUsers.Add(hand.User);

                foreach (SidePot item in sidePotList)
                {
                    List<ApplicationUser> winnersMatch = winnerUsers.Intersect(item.ContestedBy).ToList();

                    // if it doesn't have any match, it doesn't matter and we can go to the next side pot to check for a match.
                    // NOTE: we must check if item.Winners == null which means if this Side pot already taken, if it does we don't need to cehck it again.
                    if (winnersMatch.Count > 0 && item.Winners == null)
                    {
                        int amountOfEachPlayer = item.PotAmount / winnersMatch.Count;

                        item.OriginalPotAmount = item.PotAmount;

                        // Spread the money between the winners
                        foreach (var player in winnersMatch)
                        {
                            player.Chips += amountOfEachPlayer;
                            item.PotAmount -= amountOfEachPlayer;
                        }

                        item.Winners = winnersMatch;
                        item.RankName = winnerGroup[0].RankName;
                        item.WinningCards = winnerGroup[0].Cards;
                    }
                }
            }

            //Console.WriteLine(totalWinners);
            return sidePotList;
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
