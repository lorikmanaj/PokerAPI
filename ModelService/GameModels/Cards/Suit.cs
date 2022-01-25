
using System.Collections.Generic;

namespace ModelService.RoundGameModel.Cards
{
	public sealed class Suit
	{
		public static readonly Suit HEARTH = new Suit("HEARTH", InnerEnum.HEARTH); // corazones
		public static readonly Suit DIAMOND = new Suit("DIAMOND", InnerEnum.DIAMOND); // diamantes
		public static readonly Suit CLUB = new Suit("CLUB", InnerEnum.CLUB); // treboles
		public static readonly Suit SPADE = new Suit("SPADE", InnerEnum.SPADE); // picas

		private static readonly List<Suit> valueList = new List<Suit>();

		static Suit()
		{
			valueList.Add(HEARTH);
			valueList.Add(DIAMOND);
			valueList.Add(CLUB);
			valueList.Add(SPADE);
		}

		public enum InnerEnum
		{
			HEARTH,
			DIAMOND,
			CLUB,
			SPADE
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private Suit(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}



		private int value;

		

		public int NumericValue
		{
			get
			{
				return this.value;
			}
		}

		public static Suit[] values()
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

		public static Suit valueOf(string name)
		{
			foreach (Suit enumInstance in Suit.valueList)
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