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
		public async Task<Role> GetRoleAsync(long id)
		{
			return await _universalDataSource.Roles.Where(r => r.RoleID == id).FirstOrDefaultAsync();
		}

		public async Task<IList<Role>> GetRolesAsync(int skip, int take, DataRequest<Role> request)
		{
			IQueryable<Role> items = GetRoles(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Role
				{
					RoleID = r.RoleID,
					Name = r.Name,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<Role>> GetRoleKeysAsync(int skip, int take, DataRequest<Role> request)
		{
			IQueryable<Role> items = GetRoles(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Role
				{
					RoleID = r.RoleID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<Role> GetRoles(DataRequest<Role> request, bool skipSorting = false)
		{
			IQueryable<Role> items = _universalDataSource.Roles;

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
							? ((IOrderedQueryable<Role>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<Role>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetRolesCountAsync(DataRequest<Role> request)
		{
			return await GetRoles(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateRoleAsync(Role entity)
		{
			if (entity.RoleID > 0)
			{
				_universalDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.RoleID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_universalDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _universalDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteRolesAsync(params Role[] entities)
		{
			return await _universalDataSource.Roles
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
