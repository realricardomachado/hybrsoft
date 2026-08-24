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
		public async Task<Permission> GetPermissionAsync(long id)
		{
			return await _universalDataSource.Permissions.Where(r => r.PermissionID == id).FirstOrDefaultAsync();
		}

		public async Task<IList<Permission>> GetPermissionsAsync(int skip, int take, DataRequest<Permission> request)
		{
			IQueryable<Permission> items = GetPermissions(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Permission
				{
					PermissionID = r.PermissionID,
					Name = r.Name,
					DisplayName = r.DisplayName,
					Description = r.Description,
					IsEnabled = r.IsEnabled // Do not remove. It is necessary for deletion.
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<Permission>> GetPermissionKeysAsync(int skip, int take, DataRequest<Permission> request)
		{
			IQueryable<Permission> items = GetPermissions(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Permission
				{
					PermissionID = r.PermissionID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<Permission> GetPermissions(DataRequest<Permission> request, bool skipSorting = false)
		{
			IQueryable<Permission> items = _universalDataSource.Permissions;

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
							? ((IOrderedQueryable<Permission>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<Permission>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetPermissionsCountAsync(DataRequest<Permission> request)
		{
			return await GetPermissions(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdatePermissionAsync(Permission entity)
		{
			if (entity.PermissionID > 0)
			{
				_universalDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.PermissionID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_universalDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _universalDataSource.SaveChangesAsync();
		}

		public async Task<int> DeletePermissionsAsync(params Permission[] entities)
		{
			return await _universalDataSource.Permissions
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
