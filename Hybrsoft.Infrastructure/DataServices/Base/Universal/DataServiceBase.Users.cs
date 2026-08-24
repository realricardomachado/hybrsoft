using Hybrsoft.Enums;
using Hybrsoft.Infrastructure.Common;
using Hybrsoft.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hybrsoft.Infrastructure.DataServices.Base
{
	partial class DataServiceBase
	{
		public async Task<User> GetUserAsync(long id)
		{
			return await _universalDataSource.Users.Where(r => r.UserID == id).FirstOrDefaultAsync();
		}

		public async Task<User> GetUserByEmailAsync(string email)
		{
			return await _universalDataSource.Users.Where(r => r.Email == email).FirstOrDefaultAsync();
		}

		public async Task<IList<User>> GetUsersAsync(int skip, int take, DataRequest<User> request)
		{
			IQueryable<User> items = GetUsers(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new User
				{
					UserID = r.UserID,
					FirstName = r.FirstName,
					LastName = r.LastName,
					Email = r.Email,
					LastModifiedOn = r.LastModifiedOn,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<User>> GetUserKeysAsync(int skip, int take, DataRequest<User> request)
		{
			IQueryable<User> items = GetUsers(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new User
				{
					UserID = r.UserID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<User> GetUsers(DataRequest<User> request, bool skipSorting = false)
		{
			IQueryable<User> items = _universalDataSource.Users;

			// Query
			if (!string.IsNullOrEmpty(request.Query))
			{
				items = items.Where(r => EF.Functions.Like(r.SearchTerms, "%" + request.Query + "%"));
			}

			// Where
			if (request.Where != null)
			{
				items = items.Where(request.Where);
			}

			// Order By
			if (!skipSorting && request.OrderBys.Count != 0)
			{
				bool first = true;
				foreach (var (keySelector, orderBy) in request.OrderBys)
				{
					if (first)
					{
						items = orderBy == OrderBy.Desc
							? items.OrderByDescending(keySelector)
							: items.OrderBy(keySelector);
						first = false;
					}
					else
					{
						items = orderBy == OrderBy.Desc
							? ((IOrderedQueryable<User>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<User>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetUsersCountAsync(DataRequest<User> request)
		{
			return await GetUsers(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateUserAsync(User entity)
		{
			if (entity.UserID > 0)
			{
				_universalDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.UserID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_universalDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _universalDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteUsersAsync(params User[] entities)
		{
			return await _universalDataSource.Users
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
