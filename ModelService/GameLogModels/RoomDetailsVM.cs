using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.GameLogModels
{
    public class RoomDetailsVM
    {
        public List<RoundLog> RoundLog { get; set; }
        public List<RoundUserLog> RoundUserLog { get; set; }
    }
}
