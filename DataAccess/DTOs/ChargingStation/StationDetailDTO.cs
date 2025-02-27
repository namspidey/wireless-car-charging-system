using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.ChargingStation
{
    public class StationDetailDto
    {
        public ChargingStationDto Station { get; set; }
        public List<ChargingPointDto> Points { get; set; }
    }

}
