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
		public async Task<CompanyUser> GetCompanyUserAsync(long id)
		{
			return await _universalDataSource.CompanyUsers
				.Where(r => r.CompanyUserID == id)
				.Include(r => r.User)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<CompanyUser>> GetCompanyUsersAsync(int skip, int take, DataRequest<CompanyUser> request)
		{
			IQueryable<CompanyUser> items = GetCompanyUsers(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new CompanyUser
				{
					CompanyUserID = r.CompanyUserID,
					CompanyID = r.CompanyID,
					UserID = r.UserID,
					User = new User
					{
						UserID = r.User.UserID,
						FirstName = r.User.FirstName,
						LastName = r.User.LastName
					}
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<CompanyUser>> GetCompanyUserKeysAsync(int skip, int take, DataRequest<CompanyUser> request)
		{
			IQueryable<CompanyUser> items = GetCompanyUsers(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new CompanyUser
				{
					CompanyID = r.CompanyID,
					UserID = r.UserID
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<CompanyUser> GetCompanyUsers(DataRequest<CompanyUser> request, bool skipSorting = false)
		{
			IQueryable<CompanyUser> items = _universalDataSource.CompanyUsers;

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
							? ((IOrderedQueryable<CompanyUser>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<CompanyUser>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<IList<long>> GetAddedUserKeysInCompanyAsync(long parentID)
		{
			return await _universalDataSource.CompanyUsers
				.AsNoTracking()
				.Where(r => r.CompanyID == parentID)
				.Select(r => r.UserID)
				.ToListAsync();
		}

		public async Task<int> GetCompanyUsersCountAsync(DataRequest<CompanyUser> request)
		{
			return await GetCompanyUsers(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateCompanyUserAsync(CompanyUser entity)
		{
			if (entity.CompanyUserID > 0)
			{
				_universalDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.CompanyUserID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_universalDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _universalDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteCompanyUsersAsync(params CompanyUser[] entities)
		{
			return await _universalDataSource.CompanyUsers
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
