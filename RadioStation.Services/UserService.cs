using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Data;
using OnlineRadioStation.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISavedStationRepository _savedRepo;
        private readonly ILikeDislikeRepository _likeRepo;

        public UserService(
            IUserRepository userRepository,
            ISavedStationRepository savedRepo,
            ILikeDislikeRepository likeRepo)
        {
            _userRepository = userRepository;
            _savedRepo = savedRepo;
            _likeRepo = likeRepo;
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            Task.Run(() =>
            {
                try
                {
                    var fromAddress = new MailAddress("davidove122@gmail.com", "Online Radio Station");
                    var toAddress = new MailAddress(toEmail);
                    const string fromPassword = "dbir jenc bixu ncra";

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                    };

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        smtp.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }
            });
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAll().ToListAsync();
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) { return null; }
            if (user.Role == "Banned")
            {
                throw new Exception("Ваш акаунт заблоковано адміністратором.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isPasswordValid)
            {
                return user;
            }

            return null;
        }

        public async Task<User> CreateUserAsync(string username, string password, string email)
        {
            var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(username);
            if (existingUserByUsername != null)
            {
                throw new Exception("Користувач з таким ім'ям вже існує.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = username,
                PasswordHash = passwordHash,
                Email = email,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };
            _userRepository.AddEntity(newUser);
            await _userRepository.SaveChangesAsync();

            string body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                    <h2 style='color: #0d6efd;'>Ласкаво просимо, {username}!</h2>
                    <p>Ви успішно зареєструвалися на <strong>Online Radio Station</strong>.</p>
                    <p>Тепер ви можете слухати музику, створювати плейлисти улюбленого та багато іншого.</p>
                    <hr>
                    <p style='font-size: 12px; color: #888;'>Це автоматичний лист, не відповідайте на нього.</p>
                </div>";

            SendEmail(email, "Успішна реєстрація", body);

            return newUser;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetById(id);
        }

        public async Task UpdateUserRoleAsync(Guid id, string newRole)
        {
            var user = await _userRepository.GetById(id);
            if (user != null)
            {
                user.Role = newRole;
                _userRepository.UpdateEntity(user);
                await _userRepository.SaveChangesAsync();
            }
        }

        public async Task UpdateUserRoleAndStationAsync(Guid id, string newRole, Guid? stationId)
        {
            var user = await _userRepository.GetById(id);
            if (user != null)
            {
                string oldRole = user.Role;
                
                user.Role = newRole;
                user.AssignedStationId = stationId;
                
                _userRepository.UpdateEntity(user);
                await _userRepository.SaveChangesAsync();

                if (oldRole != newRole)
                {
                    string subject = "Зміна статусу акаунту";
                    string body = "";

                    if (newRole == "Banned")
                    {
                        body = "<h2 style='color: red;'>Ваш акаунт заблоковано!</h2><p>Адміністратор обмежив ваш доступ до сервісу.</p>";
                    }
                    else if (newRole == "Dj")
                    {
                        body = "<h2 style='color: green;'>Вітаємо в команді!</h2><p>Вам надано права <strong>Діджея</strong>.</p>";
                        if (stationId != null)
                        {
                            body += "<p>Зайдіть у свій кабінет, щоб побачити призначену станцію.</p>";
                        }
                    }
                    else if (newRole == "User" && oldRole == "Banned")
                    {
                        body = "<h2>Ваш акаунт розблоковано!</h2><p>Ви знову можете користуватися сервісом.</p>";
                    }

                    if (!string.IsNullOrEmpty(body))
                    {
                        SendEmail(user.Email, subject, body);
                    }
                }
            }
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.DeleteEntity(id);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<bool> ToggleSavedStationAsync(Guid userId, Guid stationId)
        {
            var existing = await _savedRepo.GetAll()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.StationId == stationId);
            if (existing != null)
            {
                await _savedRepo.DeleteEntity(existing.SavedId);
                await _savedRepo.SaveChangesAsync();
                return false;
            }
            else
            {
                var newSaved = new SavedStation
                {
                    SavedId = Guid.NewGuid(),
                    UserId = userId,
                    StationId = stationId,
                    SavedAt = DateTime.UtcNow
                };
                _savedRepo.AddEntity(newSaved);
                await _savedRepo.SaveChangesAsync();
                return true;
            }
        }

        public async Task<IEnumerable<Guid>> GetSavedStationIdsAsync(Guid userId)
        {
            return await _savedRepo.GetAll()
                .Where(s => s.UserId == userId)
                .Select(s => s.StationId)
                .ToListAsync();
        }

        public async Task<int> ToggleTrackRatingAsync(Guid userId, Guid trackId, bool isLikeBtnPressed)
        {
            var existing = await _likeRepo.GetAll()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.TrackId == trackId);
            if (existing != null)
            {
                if (existing.IsLike == isLikeBtnPressed)
                {
                    await _likeRepo.DeleteEntity(existing.LikeId);
                    await _likeRepo.SaveChangesAsync();
                    return 0;
                }
                else
                {
                    existing.IsLike = isLikeBtnPressed;
                    _likeRepo.UpdateEntity(existing);
                    await _likeRepo.SaveChangesAsync();
                    return isLikeBtnPressed ? 1 : -1;
                }
            }
            else
            {
                var newRating = new LikeDislike
                {
                    LikeId = Guid.NewGuid(),
                    UserId = userId,
                    TrackId = trackId,
                    IsLike = isLikeBtnPressed,
                    CreatedAt = DateTime.UtcNow
                };
                _likeRepo.AddEntity(newRating);
                await _likeRepo.SaveChangesAsync();
                return isLikeBtnPressed ? 1 : -1;
            }
        }

        public async Task<int> GetUserTrackRatingAsync(Guid userId, Guid trackId)
        {
            var rating = await _likeRepo.GetAll()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.TrackId == trackId);

            if (rating == null) return 0;
            return rating.IsLike ? 1 : -1;
        }

        public async Task<Dictionary<Guid, int>> GetUserTrackRatingsAsync(Guid userId)
        {
            return await _likeRepo.GetAll()
                .Where(l => l.UserId == userId)
                .ToDictionaryAsync(l => l.TrackId, l => l.IsLike ? 1 : -1);
        }

        public async Task<(int Likes, int Dislikes)> GetTrackVotesAsync(Guid trackId)
        {
            var votes = await _likeRepo.GetAll()
                .Where(l => l.TrackId == trackId)
                .ToListAsync();

            int likes = votes.Count(l => l.IsLike);
            int dislikes = votes.Count(l => !l.IsLike);

            return (likes, dislikes);
        }

        public async Task<Dictionary<Guid, (int Likes, int Dislikes)>> GetVotesForTracksAsync(IEnumerable<Guid> trackIds)
        {
            var allVotes = await _likeRepo.GetAll()
               .Where(l => trackIds.Contains(l.TrackId))
               .ToListAsync();

            var result = new Dictionary<Guid, (int Likes, int Dislikes)>();

            foreach (var trackId in trackIds)
            {
                var trackVotes = allVotes.Where(v => v.TrackId == trackId);
                int likes = trackVotes.Count(v => v.IsLike);
                int dislikes = trackVotes.Count(v => !v.IsLike);

                result[trackId] = (likes, dislikes);
            }

            return result;
        }
    }
}