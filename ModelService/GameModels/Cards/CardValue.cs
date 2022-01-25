using System.Collections.Generic;


namespace ModelService.RoundGameModel.Cards
{
	public sealed class CardValue
	{

		public static readonly CardValue SMALL_ACE = new CardValue("SMALL_ACE", InnerEnum.SMALL_ACE, 1);
		public static readonly CardValue TWO = new CardValue("TWO", InnerEnum.TWO, 2);
		public static readonly CardValue THREE = new CardValue("THREE", InnerEnum.THREE, 3);
		public static readonly CardValue FOUR = new CardValue("FOUR", InnerEnum.FOUR, 4);
		public static readonly CardValue FIVE = new CardValue("FIVE", InnerEnum.FIVE, 5);
		public static readonly CardValue SIX = new CardValue("SIX", InnerEnum.SIX, 6);
		public static readonly CardValue SEVEN = new CardValue("SEVEN", InnerEnum.SEVEN, 7);
		public static readonly CardValue EIGHT = new CardValue("EIGHT", InnerEnum.EIGHT, 8);
		public static readonly CardValue NINE = new CardValue("NINE", InnerEnum.NINE, 9);
		public static readonly CardValue TEEN = new CardValue("TEEN", InnerEnum.TEEN, 10);
		public static readonly CardValue JACK = new CardValue("JACK", InnerEnum.JACK, 11);
		public static readonly CardValue QUEEN = new CardValue("QUEEN", InnerEnum.QUEEN, 12);
		public static readonly CardValue KING = new CardValue("KING", InnerEnum.KING, 13);
		public static readonly CardValue ACE = new CardValue("ACE", InnerEnum.ACE, 14);

		private static readonly List<CardValue> valueList = new List<CardValue>();

		static CardValue()
		{
			valueList.Add(SMALL_ACE);
			valueList.Add(TWO);
			valueList.Add(THREE);
			valueList.Add(FOUR);
			valueList.Add(FIVE);
			valueList.Add(SIX);
			valueList.Add(SEVEN);
			valueList.Add(EIGHT);
			valueList.Add(NINE);
			valueList.Add(TEEN);
			valueList.Add(JACK);
			valueList.Add(QUEEN);
			valueList.Add(KING);
			valueList.Add(ACE);
		}

		public enum InnerEnum
		{
			SMALL_ACE,
			TWO,
			THREE,
			FOUR,
			FIVE,
			SIX,
			SEVEN,
			EIGHT,
			NINE,
			TEEN,
			JACK,
			QUEEN,
			KING,
			ACE
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private int value;

		internal CardValue(string name, InnerEnum innerEnum, int value)
		{
			this.value = value;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public int NumericValue
		{
			get
			{
				return this.value;
			}
		}

	//	public int getNumericValueBigACE() {
	//		return this.value == 0 ? 14 : this.value; 
	//	}


		public static CardValue[] values()
		{
			return valueList.ToArray();
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static CardValue valueOf(string name)
		{
			foreach (CardValue enumInstance in CardValue.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}