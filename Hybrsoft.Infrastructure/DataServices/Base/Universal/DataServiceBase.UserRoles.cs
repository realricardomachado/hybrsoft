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
		public async Task<UserRole> GetUserRoleAsync(long id)
		{
			return await _universalDataSource.UserRoles
				.Where(r => r.UserRoleID == id)
				.Include(r => r.Role)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<UserRole>> GetUserRolesAsync(int skip, int take, DataRequest<UserRole> request)
		{
			IQueryable<UserRole> items = GetUserRoles(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new UserRole
				{
					UserRoleID = r.UserRoleID,
					UserID = r.UserID,
					RoleID = r.RoleID,
					Role = new Role
					{
						RoleID = r.Role.RoleID,
						Name = r.Role.Name
					}
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<UserRole>> GetUserRoleKeysAsync(int skip, int take, DataRequest<UserRole> request)
		{
			IQueryable<UserRole> items = GetUserRoles(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new UserRole
				{
					UserID = r.UserID,
					RoleID = r.RoleID
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<UserRole> GetUserRoles(DataRequest<UserRole> request, bool skipSorting = false)
		{
			IQueryable<UserRole> items = _universalDataSource.UserRoles;

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
							? ((IOrderedQueryable<UserRole>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<UserRole>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<IList<long>> GetAddedRoleKeysInUserAsync(long parentID)
		{
			return await _universalDataSource.UserRoles
				.AsNoTracking()
				.Where(r => r.UserID == parentID)
				.Select(r => r.RoleID)
				.ToListAsync();
		}

		public async Task<int> GetUserRolesCountAsync(DataRequest<UserRole> request)
		{
			return await GetUserRoles(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateUserRoleAsync(UserRole entity)
		{
			if (entity.UserRoleID > 0)
			{
				_universalDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.UserRoleID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_universalDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _universalDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteUserRolesAsync(params UserRole[] entities)
		{
			return await _universalDataSource.UserRoles
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
