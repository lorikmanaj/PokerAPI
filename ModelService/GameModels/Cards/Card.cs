using System;

namespace ModelService.RoundGameModel.Cards
{
	public class Card
	{

		public Suit suit;
		public CardValue value;

		public Card()
		{
				
		}

		public Card(Suit suit, CardValue cardValue)
		{
			this.suit = suit;
			this.value = cardValue;
		}


		public Card(int suit, int value)
		{
			try
			{
				if (suit < Suit.values().Length && suit >= 0)
				{
					this.suit = Suit.values()[suit];
				}
				if (value <= CardValue.values().Length && value >= 1)
				{
					this.value = CardValue.values()[value - 1];
				}
			}
			catch (Exception ex)
			{
				// out of index probably.
				Console.WriteLine($"Out of the index? + {ex.InnerException.Message}");
			}
		}
	}

}