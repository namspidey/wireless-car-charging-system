using Azure.Core;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet.Core;
using DataAccess.DTOs.Auth;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using DataAccess.Repositories.StationRepo;
using System.Text.RegularExpressions;
using DataAccess.DTOs.CarDTO;
using Newtonsoft.Json;

namespace API.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICccdRepository _cccdRepository;
        private readonly IDriverLicenseRepository _licenseRepository;
        private readonly IBalancement _balanceRepository;
        private readonly ImageService _imageService;
        private readonly IOtpServices _otpServices;
        private readonly Random _random = new Random();
        private readonly string[] _streets = { "Nguyễn Huệ", "Lê Lợi", "Hai Bà Trưng", "Trần Hưng Đạo" };
        private readonly string[] _cities = { "Hà Nội", "TP.HCM", "Đà Nẵng", "Cần Thơ" };
        private readonly string[] _firstNames = { "An", "Bình", "Chi", "Dũng", "Giang" };
        private readonly string[] _lastNames = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng" };
        public UserService(IUserRepository userRepository, ICccdRepository cccdRepository, ImageService imageService, IOtpServices otpServices, IDriverLicenseRepository licenseRepository, IBalancement balanceRepository)
        {
            _userRepository = userRepository;
            _cccdRepository = cccdRepository;
            _imageService = imageService;
            _otpServices = otpServices;
            _licenseRepository = licenseRepository;
            _balanceRepository = balanceRepository;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CaptchaToken))
            {
                throw new ArgumentException("Captcha không được để trống", nameof(request.CaptchaToken));
            }

            var captchaValid = await VerifyCaptchaAsync(request.CaptchaToken);
            if (!captchaValid)
            {
                throw new ArgumentException("Captcha không hợp lệ", nameof(request.CaptchaToken));
            }
            if (request == null)
            {
                throw new ArgumentException("Request không được null");
            }
            ValidateRegisterRequest(request);

            var existingEmail = await _userRepository.GetUserByEmail(request.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email đã tồn tại");
            }
            var existingCccd = await _cccdRepository.GetCccdByCode(request.CccdCode.Trim());
            if (existingCccd != null)
            {
                throw new InvalidOperationException("CCCD đã tồn tại");
            }
            var existingPhone = await _userRepository.GetUserByPhone(request.PhoneNumber);
            if (existingPhone != null)
            {
                throw new InvalidOperationException("Số điện thoại đã tồn tại");
            }
            if (!IsEmailCorrect(request.Email))
            {
                throw new ArgumentException("Email không hợp lệ");
            }
            if (!IsPhoneCorrect(request.PhoneNumber))
            {
                throw new ArgumentException("Số điện thoại không hợp lệ");
            }

            // Kiểm tra độ mạnh của mật khẩu
            if (!IsPasswordCorrect(request.PasswordHash))
            {
                throw new ArgumentException("Mật khẩu không đủ mạnh");
            }

            ImageUploadResult? frontUploadResult = null;
            ImageUploadResult? backUploadResult = null;

            try
            {
                // Xác thực ảnh trước khi upload
                _imageService.ValidateImage(request.CCCDFrontImage);
                _imageService.ValidateImage(request.CCCDBackImage);

                // Upload ảnh song song
                var frontUploadTask = _imageService.UploadImagetAsync(request.CCCDFrontImage);
                var backUploadTask = _imageService.UploadImagetAsync(request.CCCDBackImage);
                await Task.WhenAll(frontUploadTask, backUploadTask);

                frontUploadResult = await frontUploadTask;
                backUploadResult = await backUploadTask;
                // Mã hóa mật khẩu
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);

                // Khởi tạo đối tượng người dùng
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = hashedPassword,
                    Fullname = request.Fullname,
                    PhoneNumber = request.PhoneNumber,
                    Dob = request.Dob,
                    RoleId = 1,
                    Gender = request.Gender,
                    Address = request.Address,
                    Status = "OTPprocess",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                // Sử dụng transaction để đảm bảo tính toàn vẹn của dữ liệu
                using var transaction = await _userRepository.BeginTransactionAsync();
                try
                {
                    if (transaction == null)
                        throw new InvalidOperationException("Không thể bắt đầu transaction");
                    // Lưu thông tin người dùng
                    await _userRepository.SaveUser(user);

                    // Tạo thông tin CCCD
                    var cccd = new Cccd
                    {
                        UserId = user.UserId,
                        Code = request.CccdCode.Trim(),
                        ImgFront = frontUploadResult.Url.ToString(),
                        ImgFrontPubblicId = frontUploadResult.PublicId,
                        ImgBack = backUploadResult.Url.ToString(),
                        ImgBackPubblicId = backUploadResult.PublicId,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };

                    await _cccdRepository.SaveCccd(cccd);
                    var balance = new Balance
                    {
                        UserId = user.UserId,
                        Balance1 = 0,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };

                    await _balanceRepository.AddBalance(balance);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        try
                        {
                            await transaction.RollbackAsync();
                        }
                        catch (Exception rollbackEx)
                        {
                            Console.WriteLine($"Lỗi rollback: {rollbackEx.Message}");
                        }
                    }
                    Console.WriteLine($"Lỗi: {ex.InnerException?.Message}");

                    throw new Exception("Đăng ký thất bại trong quá trình lưu dữ liệu", ex);
                }
            }
            catch (InvalidImageException ex)
            {
                throw new Exception("Image khong hop le", ex);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra, tiến hành xóa ảnh đã upload (nếu có)
                var deleteTasks = new List<Task>();
                if (frontUploadResult != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(frontUploadResult.PublicId));
                if (backUploadResult != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(backUploadResult.PublicId));

                await Task.WhenAll(deleteTasks);
                // Ném lỗi với thông báo tổng quát, bao gồm inner exception để dễ dàng debug
                throw new Exception("Đăng ký thất bại", ex);
            }
        }
        public async Task CreateTestAccount(string email, string password, int roleId)
        {
            // Validation logic giữ nguyên
            if (string.IsNullOrWhiteSpace(email) || email.Length > 256)
                throw new ArgumentException("Email tối đa 256 ký tự");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 128)
                throw new ArgumentException("Mật khẩu phải có từ 8 đến 128 ký tự");

            var existingEmail = await _userRepository.GetUserByEmail(email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email đã tồn tại");
            }

            // Tạo thông tin ngẫu nhiên
            var user = new User
            {
                Fullname = $"{_lastNames[_random.Next(_lastNames.Length)]} {_firstNames[_random.Next(_firstNames.Length)]}",
                PhoneNumber = GenerateRandomPhoneNumber(),
                Dob = GenerateRandomBirthDate(),
                Gender = _random.Next(2) == 0,
                Address = $"{_random.Next(1, 999)} {_streets[_random.Next(_streets.Length)]}, {_cities[_random.Next(_cities.Length)]}",
                Status = "ACTIVE",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = roleId,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            await _userRepository.SaveUser(user);
            var cccd = new Cccd
            {
                UserId = user.UserId,
                Code = GenerateRandomCccd(),
                ImgFront = "https://example.com/front.jpg",
                ImgBack = "https://example.com/back.jpg",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            await _cccdRepository.SaveCccd(cccd);
            var balance = new Balance
            {
                UserId = user.UserId,
                Balance1 = 0,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            await _balanceRepository.AddBalance(balance);
        }
        public async Task<bool> VerifyCaptchaAsync(string captchaResponse)
        {
            if (string.IsNullOrEmpty(captchaResponse))
                return false;

            var _secretKey = Environment.GetEnvironmentVariable("RECAPTCHA_SECRET_KEY");
            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new ArgumentException("Khoá bí mật không được trống");
            }
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
                {
                    { "secret", _secretKey },
                    { "response", captchaResponse }
                };

                var content = new FormUrlEncodedContent(parameters);
                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var responseString = await response.Content.ReadAsStringAsync();

                var captchaResult = JsonConvert.DeserializeObject<CaptchaResponse>(responseString);

                return captchaResult!.success;
            }
        }
        private string GenerateRandomCccd()
        {
            return _random.Next(100000000, 999999999).ToString();
        }

        private string GenerateRandomPhoneNumber()
        {
            return "0" + _random.Next(100000000, 999999999).ToString();
        }

        private DateTime GenerateRandomBirthDate()
        {
            int startYear = DateTime.Now.Year - 80;
            int endYear = DateTime.Now.Year - 18;
            int year = _random.Next(startYear, endYear);
            int month = _random.Next(1, 13);
            int day = _random.Next(1, DateTime.DaysInMonth(year, month) + 1);

            return new DateTime(year, month, day);
        }
        private void ValidateRegisterRequest(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Fullname) || request.Fullname.Length > 100)
                throw new ArgumentException("Họ và tên có tối đa 100 ký tự");

            if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 225)
                throw new ArgumentException("Email có tối đa 225 ký tự");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || request.PhoneNumber.Length > 10)
                throw new ArgumentException("Số điện thoại phải có 10 chữ số");

            if (string.IsNullOrWhiteSpace(request.Address) || request.Address.Length > 250)
                throw new ArgumentException("Địa chỉ tối đa 250 ký tự");

            var cccdCode = request.CccdCode?.Trim();
            if (string.IsNullOrWhiteSpace(cccdCode) || cccdCode.Length < 9 || cccdCode.Length > 12)
                throw new ArgumentException("CCCD phải có từ 9 đến 12 ký tự");

            if (string.IsNullOrWhiteSpace(request.PasswordHash) || request.PasswordHash.Length < 8 || request.PasswordHash.Length > 100)
                throw new ArgumentException("Mật khẩu phải có từ 8 đến 100 ký tự");
        }

        public async Task<UserDto> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new ArgumentException("Khong tìm thấy người dùng");
            }
            var UserDto = new UserDto();
            UserDto.UserId = user.UserId;
            UserDto.Email = user.Email;
            UserDto.Fullname = user.Fullname ?? "Unknown";
            UserDto.Role = new RoleDto();
            UserDto.Role.RoleId = user.RoleId;
            UserDto.Role.Name = user.Role?.RoleName ?? "Unknown";

            return UserDto;
        }
        public async Task<UserDto> GetUserByCccd(string cccd)
        {
            var user = await _userRepository.GetUserByCccd(cccd);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng");
            }
            var UserDto = new UserDto();
            UserDto.UserId = user.UserId;
            UserDto.Email = user.Email;
            UserDto.Fullname = user.Fullname ?? "Unknown";
            UserDto.Role = new RoleDto();
            UserDto.Role.RoleId = user.RoleId;
            UserDto.Role.Name = user.Role?.RoleName ?? "Unknown";

            return UserDto;
        }
        public async Task ActiveAccount(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng");
            }
            try
            {
                user.Status = "Active";
                user.UpdateAt = DateTime.UtcNow;
                await _userRepository.UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể kích hoạt tài khoản", ex);
            }
        }
        public async Task<UserDto> GetUserByPhone(string phone)
        {
            var user = await _userRepository.GetUserByPhone(phone);
            if (user == null)
            {
                throw new ArgumentException("Khong tìm thấy người dùng");
            }
            var UserDto = new UserDto();
            UserDto.UserId = user.UserId;
            UserDto.Email = user.Email;
            UserDto.Fullname = user.Fullname ?? "Unknown";
            UserDto.Role = new RoleDto();
            UserDto.Role.RoleId = user.RoleId;
            UserDto.Role.Name = user.Role?.RoleName ?? "Unknown";
            return UserDto;
        }

        public async Task<bool> ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentException("Yêu cầu không được trống");
                }
                var user = await _userRepository.GetUserByEmail(request.Email);
                if (user == null)
                {
                    throw new ArgumentException("Tài khoản không tồn tại");
                }
                var isValid = await _otpServices.verifyResetPasswordToken(request.Token, request.Email);
                if (!isValid)
                {
                    throw new ArgumentException("Token không hợp lệ");
                }
                if (!IsPasswordCorrect(request.NewPassword))
                {
                    throw new ArgumentException("Mật khẩu không đủ mạnh");
                }
                if (user.Status != "ACTIVE")
                {
                    throw new ArgumentException("Tài khoản chưa được kích hoạt");
                }
                var password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.PasswordHash = password;
                user.UpdateAt = DateTime.UtcNow;
                await _userRepository.UpdateUser(user);
                return true;

            }
            catch (Exception ex)
            {
                throw new Exception("Thay đổi mật khẩu thất bài", ex);
            }
        }

        public async Task<ProfileDTO?> GetProfileByUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Id của người dùng không hợp lệ.");
            }

            var profile = await _userRepository.GetProfileByUserId(userId);
            if (profile == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            return profile;
        }

        public async Task UpdateUserProfileAsync(int userId, RequestProfile request)
        {
            if (request == null)
            {
                throw new ArgumentException("Yêu cầu không được trống");
            }
            if (userId <= 0)
            {
                throw new ArgumentException("Id của người dùng không hợp lệ.");
            }
            ValidateUpdateProfileRequest(request);
            if (!string.IsNullOrEmpty(request.Email) && !IsEmailCorrect(request.Email))
            {
                throw new ArgumentException("Email không hợp lệ");
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber) && !IsPhoneCorrect(request.PhoneNumber))
            {
                throw new ArgumentException("Số điện thoại không hợp lệ");
            }
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }
            if (_userRepository.IsMailOrPhoneDuplicate(userId, request.Email, request.PhoneNumber))
            {
                throw new ArgumentException("Email hoặc số điện thoại đã tồn tại");
            }

            // Update user properties
            user.Fullname = request.Fullname ?? user.Fullname;
            user.Email = request.Email ?? user.Email;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.Dob = request.Dob ?? user.Dob;
            user.Gender = request.Gender ?? user.Gender;
            user.Address = request.Address ?? user.Address;
            user.UpdateAt = DateTime.UtcNow;

            await _userRepository.UpdateUser(user);
        }

        private void ValidateUpdateProfileRequest(RequestProfile request)
        {
            if (string.IsNullOrWhiteSpace(request.Fullname) || request.Fullname.Length > 100)
                throw new ArgumentException("Họ và tên tối đa 100 ký tự");

            if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 256)
                throw new ArgumentException("Email tối đa 225 ký tự");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || request.PhoneNumber.Length > 15)
                throw new ArgumentException("Số điện thoại tối đa 15 ký tự");

            if (string.IsNullOrWhiteSpace(request.Address) || request.Address.Length > 250)
                throw new ArgumentException("Địa chỉ tối đa 250 ký tự");
        }

        public async Task ChangePasswordAsync(int userId, ChangePassDTO passDTO)
        {
            if (string.IsNullOrWhiteSpace(passDTO.Password) ||
                string.IsNullOrWhiteSpace(passDTO.NewPassword) ||
                string.IsNullOrWhiteSpace(passDTO.ConfirmNewPassword))
            {
                throw new ArgumentException("Mật khẩu không thể trống.");
            }
            if (!IsPasswordCorrect(passDTO.NewPassword))
            {
                throw new ArgumentException("Mật khẩu không đủ mạnh. Vui lòng điền mật khẩu gồm 1 chữ in hoa, 1 kí tự đặc biệt, 1 só và dài ít nhất 8 kí tự");
            }
            if (passDTO.NewPassword != passDTO.ConfirmNewPassword)
            {
                throw new ArgumentException("New password and confirmation do not match.");
            }

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(passDTO.Password, user.PasswordHash))
            {
                throw new ArgumentException("Mật khẩu hiện tại không đúng.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passDTO.NewPassword);
            await _userRepository.UpdateUser(user);
        }

        public async Task<List<User>> GetUsersByEmailOrPhoneAsync(string search)
        {
            var users = await _userRepository.GetUserByEmailOrPhone(search);

            if (users == null || users.Count == 0)
                return users;

            // Giả sử chỉ cần kiểm tra license của người dùng đầu tiên
            var userId = users[0].UserId;
            var hasLicense = await _userRepository.HavingDriverLicenseYet(userId);


            if (!hasLicense)
            {
                throw new Exception("Tài xế chưa đăng ký hoặc được duyệt giấy phép lái xe");
            }

            return users;
        }
        public async Task<DriverLicenseDTO> GetLicenseByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Mã số bằng lái không hợp lệ", nameof(code));
            }

            var existingLicense = await _licenseRepository.GetLicenseByCode(code);
            if (existingLicense == null)
            {
                throw new ArgumentException("Không tìm thấy bằng lái");
            }
            var user = await _userRepository.GetUserById(existingLicense.UserId);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng");
            }

            var licenseDTO = new DriverLicenseDTO
            {
                LicenseId = existingLicense.DriverLicenseId,
                LicenseNumber = existingLicense.Code ?? "N/A",
                Class = existingLicense.Class ?? "N/A",
                FrontImageUrl = existingLicense.ImgFront ?? "N/A",
                BackImageUrl = existingLicense.ImgBack ?? "N/A",
                Status = existingLicense.Status ?? "N/A",
                CreatedAt = existingLicense.CreateAt,
                UpdatedAt = existingLicense.UpdateAt,
                User = new UserSimpleDTO
                {
                    UserId = user.UserId,
                    Fullname = user.Fullname ?? "",
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                }
            };
            return licenseDTO;
        }

        public async Task AddDriverLicenseAsync(int userId, DriverLicenseRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Yêu cầu không được trống");
            }
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng");
            }

            ImageUploadResult? frontUpload = null;
            ImageUploadResult? backUpload = null;
            var existingLicenses = await _licenseRepository.GetLicensesByUserId(userId);
            if (existingLicenses != null)
            {
                foreach (var license in existingLicenses)
                {
                    if (license.Code == request.LicenseNumber)
                    {
                        if (license.Status == "DELETE")
                        {
                            _imageService.ValidateImage(request.LicenseFrontImage);
                            _imageService.ValidateImage(request.LicenseBackImage);
                            if (license.ImgFrontPubblicId != null && license.ImgBackPubblicId != null)
                            {
                                await _imageService.DeleteImageAsync(license.ImgFrontPubblicId);
                                await _imageService.DeleteImageAsync(license.ImgBackPubblicId);
                            }
                            frontUpload = await _imageService.UploadImagetAsync(request.LicenseFrontImage);
                            backUpload = await _imageService.UploadImagetAsync(request.LicenseBackImage);
                            license.ImgFront = frontUpload.Url.ToString();
                            license.ImgFrontPubblicId = frontUpload.PublicId;
                            license.ImgBack = backUpload.Url.ToString();
                            license.ImgBackPubblicId = backUpload.PublicId;
                            license.Code = request.LicenseNumber;
                            license.Status = "PENDING";
                            license.Class = request.Class;
                            license.UpdateAt = DateTime.UtcNow;


                            await _licenseRepository.UpdateLicense(license);
                            return;
                        }
                        else if (license.Status == "PENDING")
                        {
                            throw new ArgumentException("Bằng lái bạn muốn thêm đang trong hàng chờ");
                        }
                        else if (license.Status == "APPROVED")
                        {
                            throw new ArgumentException("Bằng lái bạn muốn thêm đã được chấp nhận");
                        }
                        else if (license.Status == "REJECTED")
                        {
                            throw new ArgumentException("Bằng lái bạn muốn thêm đã bị từ chối");
                        }
                        else if (license.Status == "BLOCKED")
                        {
                            throw new ArgumentException("Bằng lái bạn muốn thêm đã bị vô hiệu hoá");
                        }
                        else
                        {
                            throw new ArgumentException("Bằng lái bạn muốn thêm đã tồn tại");
                        }
                    }
                }
            }
            try
            {
                _imageService.ValidateImage(request.LicenseFrontImage);
                _imageService.ValidateImage(request.LicenseBackImage);
                frontUpload = await _imageService.UploadImagetAsync(request.LicenseFrontImage);
                backUpload = await _imageService.UploadImagetAsync(request.LicenseBackImage);
                var license = new DriverLicense
                {
                    UserId = userId,
                    Code = request.LicenseNumber,
                    Class = request.Class,
                    ImgFront = frontUpload.Url.ToString(),
                    ImgFrontPubblicId = frontUpload.PublicId,
                    ImgBack = backUpload.Url.ToString(),
                    ImgBackPubblicId = backUpload.PublicId,
                    Status = "PENDING",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };
                using var transaction = await _licenseRepository.BeginTransactionAsync();
                try
                {
                    await _licenseRepository.SaveLicense(license);

                    var documentReview = new DocumentReview
                    {
                        UserId = userId,
                        ReviewType = "DRIVER_LICENSE",
                        DriverLicenseId = license.DriverLicenseId,
                        Status = "PENDING",
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };
                    await _userRepository.AddDocumentRequest(documentReview);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                var deleteTasks = new List<Task>();
                if (frontUpload != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(frontUpload.PublicId));
                if (backUpload != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(backUpload.PublicId));
                await Task.WhenAll(deleteTasks);
                throw new Exception("Thêm bằng lái thất bại", ex);
            }

        }
        public async Task UpdateDriverLiscense(string licensecode, DriverLicenseRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Yều cầu không được thiếu");
            }
            var license = await _licenseRepository.GetLicenseByCode(licensecode);
            if (license == null)
            {
                throw new ArgumentException("Không tìm thấy bằng lái");
            }
            var existingLicenses = await _licenseRepository.GetLicenseByCode(request.LicenseNumber);
            if (existingLicenses != null && existingLicenses.Code != licensecode)
            {
                throw new ArgumentException("Bằng lái mà bạn muốn cập nhật đã tồn tại trong hệ thống");
            }

            ImageUploadResult? newFront = null;
            ImageUploadResult? newBack = null;
            var oldFront = license.ImgFrontPubblicId;
            var oldBack = license.ImgBackPubblicId;
            try
            {
                if (request.LicenseFrontImage != null)
                {
                    _imageService.ValidateImage(request.LicenseFrontImage);
                    newFront = await _imageService.UploadImagetAsync(request.LicenseFrontImage);
                }

                if (request.LicenseBackImage != null)
                {
                    _imageService.ValidateImage(request.LicenseBackImage);
                    newBack = await _imageService.UploadImagetAsync(request.LicenseBackImage);
                }


                // Cập nhật thông tin
                license.Code = request.LicenseNumber;
                license.Class = request.Class;
                license.UpdateAt = DateTime.UtcNow;
                if (newFront != null)
                {
                    license.ImgFront = newFront.Url.ToString();
                    license.ImgFrontPubblicId = newFront.PublicId;
                }

                if (newBack != null)
                {
                    license.ImgBack = newBack.Url.ToString();
                    license.ImgBackPubblicId = newBack.PublicId;
                }
                using var transaction = await _licenseRepository.BeginTransactionAsync();
                try
                {
                    await _licenseRepository.UpdateLicense(license);
                    // Xóa ảnh cũ sau khi update thành công
                    var deleteTasks = new List<Task>();
                    if (oldFront != null) deleteTasks.Add(_imageService.DeleteImageAsync(oldFront));
                    if (oldBack != null) deleteTasks.Add(_imageService.DeleteImageAsync(oldBack));


                    await Task.WhenAll(deleteTasks);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                var deleteTasks = new List<Task>();
                if (newFront != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(newFront.PublicId));
                if (newBack != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(newBack.PublicId));
                await Task.WhenAll(deleteTasks);
                throw new Exception("Cập nhật bằng lái thất bài");
            }
        }
        public async Task<IEnumerable<DriverLicenseResponse>> GetActiveDriverLicensesAsync(int userId)
        {
            var licenses = await _licenseRepository.GetLicensesByUserId(userId);
            if (licenses == null || !licenses.Any())
            {
                return Enumerable.Empty<DriverLicenseResponse>();
            }

            var activeLicenses = licenses.Where(l => l.Status == "APPROVED").ToList();
            var responses = new List<DriverLicenseResponse>();

            foreach (var license in activeLicenses)
            {
                try
                {
                    if (string.IsNullOrEmpty(license.ImgBack))
                    {
                        throw new ArgumentNullException(nameof(license.ImgBack), "Đường đẵn ảnh mặt sau không được trống");
                    }

                    var qrResult = await _imageService.ReadQrCodeUrl(license.ImgBack);
                    responses.Add(new DriverLicenseResponse(license, qrResult));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong quá trình lấy bằng {license.Code}: {ex.Message}");
                }
            }

            return responses;
        }

        public async Task DeleteDriverLicenseAsync(string licenseCode)
        {
            var license = await _licenseRepository.GetLicenseByCode(licenseCode);
            if (license == null)
            {
                throw new ArgumentException("Không tìm thấy bằng lái");
            }

            license.Status = "DELETE";
            license.UpdateAt = DateTime.UtcNow;

            using var transaction = await _licenseRepository.BeginTransactionAsync();
            try
            {
                await _licenseRepository.UpdateLicense(license);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        } 
        public async Task BlockedDriverLicenseAsync(string licenseCode)
        {
            var license = await _licenseRepository.GetLicenseByCode(licenseCode);
            if (license == null)
            {
                throw new ArgumentException("Không tìm thấy bằng lái");
            }

            license.Status = "BLOCKED";
            license.UpdateAt = DateTime.UtcNow;

            using var transaction = await _licenseRepository.BeginTransactionAsync();
            try
            {
                await _licenseRepository.UpdateLicense(license);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task ActiveDriverLicenseAsync(string licenseCode)
        {
            var license = await _licenseRepository.GetLicenseByCode(licenseCode);
            if (license == null)
            {
                throw new ArgumentException("Không tìm thấy bằng lái");
            }

            license.Status = "APPROVED";
            license.UpdateAt = DateTime.UtcNow;

            using var transaction = await _licenseRepository.BeginTransactionAsync();
            try
            {
                await _licenseRepository.UpdateLicense(license);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateDriverlicenseOperator(string licenseCode, string newCode, string fullname, string level)
        {
            var license = await _licenseRepository.GetLicenseByCode(licenseCode);
            if (license == null)
            {
                throw new ArgumentException("Không tìm thấy bằng lái");
            }

            var user = await _userRepository.GetUserById(license.UserId);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng");
            }

            if (!string.IsNullOrEmpty(fullname))
            {
                user.Fullname = fullname;
            }
            if (!string.IsNullOrEmpty(level))
            {
                license.Class = level;
            }
            if (!string.IsNullOrEmpty(newCode))
            {
                license.Code = newCode;
            }
            license.UpdateAt = DateTime.UtcNow;

            using var transaction = await _licenseRepository.BeginTransactionAsync();
            try
            {
                await _userRepository.UpdateUser(user);
                await _licenseRepository.UpdateLicense(license);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PagedResultD<DriverLicenseDTO>> GetLicenseList(int pageNumber, int pageSize, DriverLicenseFilter filter)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Số lượng item trong 1 trang phải lớn hơn 0", nameof(pageSize));
            }
            return await _licenseRepository.GetPagedLicensesAsync(pageNumber, pageSize, filter);
        }
        public PagedResult<UserDto> GetUsers(string? searchQuery, string? status, int? roleId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Số lượng item trong 1 trang phải lớn hơn 0", nameof(pageSize));
            }
            return _userRepository.GetUsers(searchQuery, status, roleId, pageNumber, pageSize);
        }

        public async Task ChangeUserStatusAsync(int userId, string newStatus)
        {
            await _userRepository.ChangeUserStatusAsync(userId, newStatus);
        }
        public async Task DeleteUserReal(int userId)
        {
            await _userRepository.DeleteUserReal(userId);
        }
        public async Task PendingRegisterUserViaOtpAsync(string email, string otpToken)
        {
            // 1. Lấy user theo email
            var user = await _userRepository.GetUserByEmail(email);
            if (user is null)
                throw new ArgumentException("Tài khoản không tồn tại", nameof(email));

            // 2. Chỉ cho phép khi user đang ở trạng thái chờ OTP
            if (user.Status != "OTPprocess")
                throw new InvalidOperationException("Tài khoản không trong trạng thái chờ xác thực OTP.");

            // 3. Kiểm tra OTP
            var isValidOtp = await _otpServices.VerifyOtpAsync(email, otpToken);
            if (!isValidOtp)
                throw new ArgumentException("OTP không hợp lệ", nameof(otpToken));

            // 4. Lấy CCCD đã upload
            var existingCccd = user.Cccds.FirstOrDefault();
            if (existingCccd is null)
                throw new InvalidOperationException("Chưa có hồ sơ CCCD để xác thực.");

            // 5. Tạo DocumentReview mới
            var now = DateTime.UtcNow;
            var review = new DocumentReview
            {
                UserId = user.UserId,
                CccdId = existingCccd.CccdId,
                ReviewType = "CCCD",
                Status = "PENDING",
                CreateAt = now,
                UpdateAt = now
            };

            // 6. Ghi vào DB trong transaction
            using var transaction = await _userRepository.BeginTransactionAsync();
            try
            {
                await _userRepository.AddDocumentRequest(review);

                user.Status = "PENDING";
                user.UpdateAt = now;
                await _userRepository.UpdateUser(user);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private bool IsEmailCorrect(string email)
        {
            string regex = @"^(?=.{1,64}@)[A-Za-z0-9_-]+(\.[A-Za-z0-9_-]+)*@[^-][A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*(\.[A-Za-z]{2,})$";
            return Regex.IsMatch(email, regex, RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
        }

        private bool IsPhoneCorrect(string phone)
        {
            string regex = @"^(0)(3[2-9]|5[2689]|7[06789]|8[1-9]|9[0-9]|2[0-9])\d{7}$";
            return Regex.IsMatch(phone, regex, RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
        }
        private bool IsPasswordCorrect(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var hasMinimumLength = password.Length >= 6;
            var hasUpperCase = password.Any(char.IsUpper);
            var hasNumber = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

            var allowedSpecialChars = "!@#$%^&*(),.?\":{}|<>";
            var hasValidSpecialChar = password.Any(c => allowedSpecialChars.Contains(c));

            return hasMinimumLength &&
                   hasUpperCase &&
                   hasNumber &&
                   hasSpecialChar &&
                   hasValidSpecialChar;
        }

        public async Task<string> VerifyAndGenResetPasswordToken(string email, string otpCode)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new ArgumentException("Tài khoản không tồn tại", nameof(email));
            }
            var isValid = await _otpServices.VerifyOtpAsync(email, otpCode);
            if (!isValid)
            {
                throw new ArgumentException("Token không hợp lệ", nameof(otpCode));
            }
            var token = await _otpServices.genResetPasswordToken(email);
            return token;
        }

        public async Task<bool> HavingDriverLicenseYet(int userId)
        {
            return await _userRepository.HavingDriverLicenseYet(userId);
        }
    }
}
