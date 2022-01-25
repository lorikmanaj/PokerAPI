using PokerLogic.Models.Accesing;
using PokerLogic.Models.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketService.Utils
{
    class EpprGameProto
    {
		public virtual DefinePosition getDefinePositionSchema(IList<int> freeSpaces)
		{
			DefinePosition @out = new DefinePosition();
			@out.positions = freeSpaces;
			return @out;
		}

		public RejectFullyfied getRejectFullyfiedSchema()
		{
			return new RejectFullyfied();
		}

		public virtual RejectFullyfied RejectFullyfiedSchema
		{
			get
			{
				return new RejectFullyfied();
			}
		}

		public virtual Ingress getIngressSchema(UserData userData, int position)
		{
			Ingress ingerss = new Ingress();
			ingerss.chips = userData.chips;
			ingerss.position = position;
			return ingerss;
		}

		public virtual Announcement getAnnouncementSchema(UserData userData, int position)
		{
			Announcement @out = new Announcement();
			@out.position = position;
			@out.chips = userData.chips;
			@out.avatar = "https://images.pexels.com/photos/3200602/pexels-photo-3200602.jpeg?auto=compress&cs=tinysrgb&dpr=2&h=650&w=940";
			@out.user = userData.dataBlock.UserName;
			@out.userID = userData.dataBlock.PlayerId;
			return @out;
		}

		public virtual RejectedPosition getRejectedPositionSchema(IList<int> freeSpaces)
		{
			RejectedPosition position = new RejectedPosition();
			position.positions = freeSpaces;
			return position;
		}

	}
}
