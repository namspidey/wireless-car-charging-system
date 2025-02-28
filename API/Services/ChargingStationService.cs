using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Repositories;

namespace API.Services
{
    public class ChargingStationService
    {
        private readonly IChargingStationRepository _stationRepository;
        private readonly IChargingPointRepository _pointRepository;

        public ChargingStationService(IChargingStationRepository stationRepository, IChargingPointRepository pointRepository)
        {
            _pointRepository = pointRepository;
            _stationRepository = stationRepository;
        }

        public PagedResult<ChargingStationDto> GetChargingStations(string? keyword, decimal userLat, decimal userLng, int page, int pageSize)
        {
            return _stationRepository.GetAllStation(keyword, userLat, userLng, page, pageSize);
        }

        public StationDetailDto? GetStationDetails(int stationId, int page, int pageSize)
        {
            // Lấy thông tin trạm sạc
            var station = _stationRepository.GetStationById(stationId);

            // Nếu không tìm thấy trạm sạc, return null
            if (station == null) return null;

            // Lấy danh sách điểm sạc của trạm đó
            var points = _pointRepository.GetAllPointsByStation(stationId, page, pageSize);

            // Gộp dữ liệu vào DTO mới
            return new StationDetailDto
            {
                Station = station,
                Points = points
            };
        }


        public void SaveChargingStation(NewChargingStationDto chargingStation)
        {
            //_stationRepository.SaveStation(chargingStation);
        }

        public void UpdateChargingStation(UpdateChargingStationDto chargingStation)
        {
            _stationRepository.UpdateStation(chargingStation);
        }

        public void DeleteChargingStation(int stationId)
        {
            _stationRepository.DeleteStation(stationId);
        }


    }
}
