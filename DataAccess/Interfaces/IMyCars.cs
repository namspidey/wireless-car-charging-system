using DataAccess.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IMyCars
    {
        List<MyCarsDTO> getCarByOwner(int userId);

        CarDetailDTO getCarDetailById(int carId);

        ChargingStatusDTO GetChargingStatusById(int carId);


        List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId);
    }
}
