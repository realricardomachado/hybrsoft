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
		public async Task<Company> GetCompanyAsync(long id)
		{
			return await _universalDataSource.Companies
				.Where(r => r.CompanyID == id)
				.Include(r => r.Country)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<Company>> GetCompaniesAsync(int skip, int take, DataRequest<Company> request)
		{
			IQueryable<Company> items = GetCompanies(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Company
				{
					CompanyID = r.CompanyID,
					LegalName = r.LegalName,
					TradeName = r.TradeName,
					FederalRegistration = r.FederalRegistration,
					Country = new Country(),
					Phone = r.Phone
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<Company>> GetCompanyKeysAsync(int skip, int take, DataRequest<Company> request)
		{
			IQueryable<Company> items = GetCompanies(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Company
				{
					CompanyID = r.CompanyID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<Company> GetCompanies(DataRequest<Company> request, bool skipSorting = false)
		{
			IQueryable<Company> items = _universalDataSource.Companies;

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
							? ((IOrderedQueryable<Company>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<Company>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetCompaniesCountAsync(DataRequest<Company> request)
		{
			return await GetCompanies(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateCompanyAsync(Company entity)
		{
			if (entity.CompanyID > 0)
			{
				_universalDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.CompanyID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_universalDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _universalDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteCompaniesAsync(params Company[] entities)
		{
			return await _universalDataSource.Companies
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
