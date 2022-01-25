using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ModelService.RoundGameModel.Cards
{

	public class CardComparator : IComparer<Card>
	{

		public virtual int Compare(Card a, Card b)
		{
			return a.value.NumericValue - b.value.NumericValue;
		}

	}

}
