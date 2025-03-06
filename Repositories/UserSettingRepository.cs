using NETCORE.Data.Entities;
using NETCORE.Models;

namespace NETCORE.Repositories
{
    public interface IUserSettingRepository
    {
        Task<IEnumerable<UserSetting>> GetAllUserSettingsAsync();
        Task<Pagination<UserSetting>> GetUserSettingsPagedAsync(int pageNumber, int pageSize, string name);
        Task<UserSetting?> GetUserSettingByUserIdAsync(string id);
        Task CreateUserSettingAsync(UserSetting userSetting);
        Task UpdateUserSettingAsync(UserSetting userSetting);
        Task DeleteUserSettingAsync(string id);
        Task<bool> UserSettingExistsAsync(string id);
    }

    public class UserSettingRepository : IUserSettingRepository
    {
        private readonly UnitOfWork _unitOfWork;
        public UserSettingRepository(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserSetting>> GetAllUserSettingsAsync()
        {
            return await _unitOfWork._userSettingRepository.GetAllAsync();
        }

        public async Task<Pagination<UserSetting>> GetUserSettingsPagedAsync(int pageNumber, int pageSize, string name)
        {
            // Calculate the number of items to skip based on the page number and page size
            int skipCount = (pageNumber - 1) * pageSize;

            // Get the total count of records that match the filter (for pagination calculation)
            int totalRecords = await _unitOfWork._userSettingRepository.CountAsync(
                predicate: c => c.User!.Email!.Contains(name)
            );

            // Apply the filtering, pagination, and sorting
            var userSettings = await _unitOfWork._userSettingRepository.GetManyAsync(
                predicate: c => c.User!.Email!.Contains(name),  // Filter by email
                queryOperation: query => query.Skip(skipCount).Take(pageSize) // Apply paging and sorting
            );

            // Return the Pagination object containing the results and pagination information
            return new Pagination<UserSetting>
            {
                PageIndex = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Items = userSettings.ToList() // The paged list of userSettings
            };
        }


        public async Task<UserSetting?> GetUserSettingByUserIdAsync(string id)
        {
            return await _unitOfWork._userSettingRepository
                .FindAsync(c => c.UserId == id);
        }

        public async Task CreateUserSettingAsync(UserSetting userSetting)
        {
            _unitOfWork._userSettingRepository.Add(userSetting);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateUserSettingAsync(UserSetting userSetting)
        {
            _unitOfWork._userSettingRepository.Update(userSetting);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteUserSettingAsync(string id)
        {
            var userSetting = await _unitOfWork._userSettingRepository.GetByIdAsync(id);
            if (userSetting != null)
            {
                _unitOfWork._userSettingRepository.Remove(userSetting);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> UserSettingExistsAsync(string id)
        {
            return await _unitOfWork._userSettingRepository.ExistsAsync(id);
        }
    }
}