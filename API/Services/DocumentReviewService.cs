using DataAccess.DTOs;
using DataAccess.Interfaces;
using DataAccess.Repositories.StationRepo;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace API.Services
{
    public class DocumentReviewService
    {
        private readonly IDocumentReviewRepository _repository;
        private readonly ICccdRepository _cccdRepo;
        private readonly IDriverLicenseRepository _driverLicenseRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMyCars _carRepo;

        public DocumentReviewService(IDocumentReviewRepository repository, ICccdRepository cccdRepo, IDriverLicenseRepository driverLicenseRepo, IUserRepository userRepository, IMyCars carRepo)
        {
            _repository = repository;
            _cccdRepo = cccdRepo;
            _driverLicenseRepo = driverLicenseRepo;
            _userRepo = userRepository;
            _carRepo = carRepo;
        }

        public PagedResult<DocumentReviewDto> GetAllDocumentReview(string type, string? status, int page, int pageSize)
        {
            return _repository.GetAllDocumentReview(type, status, page, pageSize);
            
        }

        public async Task<bool> UpdateReviewInfoAsync(UpdateDocumentReviewDto dto, int currentUserId)
        {
            await _repository.UpdateReviewInfoAsync(dto, currentUserId);

            int reviewID = dto.Id;
            var document = await _repository.GetDocumentReviewById(reviewID);

            switch (dto.Type)
            {
                case "CCCD":
                    await _userRepo.ChangeUserStatusAsync(document.UserId, "ACTIVE");
                    break;
                case "DRIVER_LICENSE":
                    await _driverLicenseRepo.ChangeLicenseStatusAsync(document.DriverLicenseId, "APPROVED");
                    break;
                case "CAR_LICENSE":
                    await _carRepo.ChangeCarStatusAsync(document.CarId, "APPROVED");
                    break;
            }

            return true;
        }

        public async Task<DocumentDetailDto?> GetDocumentDetail(int? documentId)
        {
            if (!documentId.HasValue)
                return null;

            var document = await _repository.GetDocumentReviewById(documentId.Value);
            if (document == null)
                return null;

            if (document.CccdId.HasValue)
            {
                var cccd = await _cccdRepo.GetCccdById(document.CccdId.Value);
                if (cccd == null)
                    return null;

                return new DocumentDetailDto
                {
                    Type = "CCCD",
                    DocumentId = cccd.CccdId,
                    Code = cccd.Code,
                    Comments = document.Comments,
                    ImageFront = cccd.ImgFront,
                    ImageBack = cccd.ImgBack,
                    FullName = cccd.User.Fullname,
                    DoB = cccd.User.Dob.Value.ToString("dd/MM/yyyy"),
                    Address = cccd.User.Address,
                    Gender = (bool)cccd.User.Gender ? "Nam" : "Nữ"
                };
            }

            if (document.DriverLicenseId.HasValue)
            {
                var license = await _driverLicenseRepo.GetLicensesById(document.DriverLicenseId.Value);
                if (license == null)
                    return null;

                return new DocumentDetailDto
                {
                    Type = "DRIVER_LICENSE",
                    DocumentId = license.DriverLicenseId,
                    Code = license.Code,
                    Comments = document.Comments,
                    ImageFront = license.ImgFront,
                    ImageBack = license.ImgBack,
                    Class = license.Class,
                    FullName = license.User.Fullname,
                    DoB = license.User.Dob.Value.ToString("dd/MM/yyyy"),
                    Address = license.User.Address
                };
            }

            if (document.CarId.HasValue)
            {
                var car = await _carRepo.GetCarById(document.CarId.Value);
                if (car == null)
                    return null;

                var user = car.UserCars.FirstOrDefault()?.User;

                return new DocumentDetailDto
                {
                    Type = "CAR_LICENSE",
                    DocumentId = car.CarId,
                    Code = car.LicensePlate,
                    Comments = document.Comments,
                    ImageFront = car.ImgFront,
                    ImageBack = car.ImgBack,
                    FullName = user.Fullname,
                    Address = user.Address,
                    Brand = car.CarModel.Brand,
                    Color = car.CarModel.Color
                };
            }

            return null;
        }
    }
}
