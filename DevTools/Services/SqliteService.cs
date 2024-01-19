using FreeSql;
using DevTools.Common;
using DevTools.Models;

namespace DevTools.Services
{
    public class SqliteService
    {
        private readonly IBaseRepository<User> userRepo;
        private readonly IBaseRepository<UserLoginRecord> userLoginRecordRepo;
        private readonly IBaseRepository<SearchRecord> searchRecordRepo;
        private readonly IBaseRepository<SearchRemark> searchRemarkRepo;

        public SqliteService(IFreeSql freeSql)
        {
            userRepo = freeSql.GetRepository<User>();
            userLoginRecordRepo = freeSql.GetRepository<UserLoginRecord>();
            searchRecordRepo = freeSql.GetRepository<SearchRecord>();
            searchRemarkRepo = freeSql.GetRepository<SearchRemark>();
        }

        #region User

        public async Task<User?> AddUserAsync(User user)
        {
            if (user == null) return null;
            return await userRepo.InsertAsync(user);
        }

        public async Task<bool> UpdateUserDefaultAsync(User user)
        {
            if (user == null || user.Id <= 0) return false;
            await userRepo.Orm.Update<User>().Where(u => u.Env == user.Env.GetHashCode()).Set(u => new User { Default = false }).ExecuteAffrowsAsync();
            return await userRepo.Orm.Update<User>(user.Id).Set(u => new User { Default = user.Default }).ExecuteAffrowsAsync() > 0;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null) return false;
            return await userRepo.UpdateAsync(user) > 0;
        }

        public async Task<bool> DeleteUserAsync(User user)
        {
            if (user == null) return false;
            return await userRepo.DeleteAsync(user) > 0;
        }

        public async Task<User?> QueryUserAsync(EnvEnum env)
        {
            return await userRepo.Select.Where(u => u.Env == env.GetHashCode()).ToOneAsync();
        }

        public async Task<List<User>?> QueryUsersAsync(EnvEnum? env)
        {
            if (!env.HasValue) return await userRepo.Select.ToListAsync();
            return await userRepo.Select.Where(u => u.Env == env.GetHashCode()).ToListAsync();
        }

        #endregion

        #region User Login Record

        public async Task<UserLoginRecord?> AddUserLoginRecordAsync(UserLoginRecord cookie)
        {
            if (cookie == null) return null;
            return await userLoginRecordRepo.InsertAsync(cookie);
        }

        public async Task<UserLoginRecord?> QueryLatestUserLoginRecordAsync(EnvEnum env)
        {
            var now = DateTime.Now;
            return await userLoginRecordRepo.Select
                .Where(u => u.Env == env.GetHashCode())
                .Where(u => u.ExpiredDate > now)
                .OrderByDescending(u => u.ExpiredDate)
                .ToOneAsync();
        }

        #endregion

        #region Search Records

        public async Task<List<SearchRecord>?> AddSearchRecordsAsync(List<SearchRecord> records)
        {
            if (records == null || records.Count == 0) return null;
            return await searchRecordRepo.InsertAsync(records);
        }

        public async Task<List<SearchRecord>?> QuerySearchRecordsPageAsync(SearchRecordPagingInfo page)
        {
            var keyWord = (page.KeyWord ?? string.Empty).Trim();
            return await searchRecordRepo
                    .Where(r => r.Env == page.Env)
                    .Where(r => r.RecordDate >= DateTime.Now.AddDays(-7)) // 查一个星期内的
                    .WhereIf(!string.IsNullOrEmpty(page.KeyWord), r => r.ClientIp.Trim().Contains(keyWord) || r.ServiceName.Trim().Contains(keyWord) || r.KeyWord.Trim().Contains(keyWord) || r.Query.Trim().Contains(keyWord))
                    .OrderByDescending(c => c.RecordDate)
                    .Page(page)
                    .ToListAsync();
        }

        #endregion

        #region Search Remarks

        public async Task<List<SearchRemark>?> AddSearchRemarksAsync(List<SearchRemark> remarks)
        {
            if (remarks == null || remarks.Count == 0) return null;
            return await searchRemarkRepo.InsertAsync(remarks);
        }

        public async Task<bool> DeleteSearchRemarksAsync(long id)
        {
            if (id <= 0) return false;
            return await searchRemarkRepo.DeleteAsync(r => r.Id == id) > 0;
        }

        public async Task<SearchRemark?> QuerySearchRemarkAsync(SearchRemark remark)
        {
            return await searchRemarkRepo
                    .Where(r => r.ClientIp.Equals(remark.ClientIp))
                    .Where(r => r.ServiceName.Equals(remark.ServiceName))
                    .Where(r => r.KeyWord.Equals(remark.KeyWord))
                    .Where(r => r.Query.Equals(remark.Query))
                    .ToOneAsync();
        }
        public async Task<List<SearchRemark>?> QuerySearchRemarksPageAsync(SearchRemarkPagingInfo page)
        {
            var keyWord = (page.KeyWord ?? string.Empty).Trim();
            return await searchRemarkRepo
                    .WhereIf(!string.IsNullOrEmpty(page.KeyWord), r => r.Desc.Trim().Contains(keyWord) || r.ClientIp.Trim().Contains(keyWord) || r.ServiceName.Trim().Contains(keyWord) || r.KeyWord.Trim().Contains(keyWord) || r.Query.Trim().Contains(keyWord))
                    .OrderByDescending(c => c.CreateDate)
                    .Page(page)
                    .ToListAsync();
        }

        #endregion

    }
}
