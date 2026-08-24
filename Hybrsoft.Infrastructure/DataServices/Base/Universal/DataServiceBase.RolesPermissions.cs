using Hybrsoft.Enums;
using Hybrsoft.Infrastructure.Common;
using Hybrsoft.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Hybrsoft.Infrastructure.DataServices.Base
{
	partial class DataServiceBase
	{
		public async Task<RolePermission> GetRolePermissionAsync(long id)
		{
			return await _universalDataSource.RolePermissions
				.Where(r => r.RolePermissionID == id)
				.Include(r => r.Permission)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<RolePermission>> GetRolePermissionsAsync(int skip, int take, DataRequest<RolePermission> request)
		{
			IQueryable<RolePermission> items = GetRolePermissions(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new RolePermission
				{
					RolePermissionID = r.RolePermissionID,
					RoleID = r.RoleID,
					PermissionID = r.PermissionID,
					Permission = new Permission
					{
						PermissionID = r.Permission.PermissionID,
						DisplayName = r.Permission.DisplayName
					}
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<RolePermission>> GetRolePermissionKeysAsync(int skip, int take, DataRequest<RolePermission> request)
		{
			IQueryable<RolePermission> items = GetRolePermissions(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new RolePermission
				{
					RoleID = r.RoleID,
					PermissionID = r.PermissionID
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<RolePermission> GetRolePermissions(DataRequest<RolePermission> request, bool skipSorting = false)
		{
			IQueryable<RolePermission> items = _universalDataSource.RolePermissions;

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
							? ((IOrderedQueryable<RolePermission>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<RolePermission>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<IList<long>> GetAddedPermissionKeysInRoleAsync(long parentID)
		{
			return await _universalDataSource.RolePermissions
				.AsNoTracking()
				.Where(r => r.RoleID == parentID)
				.Select(r => r.PermissionID)
				.ToListAsync();
		}

		public async Task<int> GetRolePermissionsCountAsync(DataRequest<RolePermission> request)
		{
			return await GetRolePermissions(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateRolePermissionAsync(RolePermission entity)
		{
			if (entity.RolePermissionID > 0)
			{
				_universalDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.RolePermissionID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_universalDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _universalDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteRolePermissionsAsync(params RolePermission[] entities)
		{
			return await _universalDataSource.RolePermissions
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
