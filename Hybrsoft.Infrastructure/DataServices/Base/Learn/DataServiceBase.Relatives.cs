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
		public async Task<Relative> GetRelativeAsync(long id)
		{
			return await _learnDataSource.Relatives
				.Where(r => r.RelativeID == id)
				.Include(r => r.RelativeType)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<Relative>> GetRelativesAsync(int skip, int take, DataRequest<Relative> request)
		{
			IQueryable<Relative> items = GetRelatives(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Relative
				{
					RelativeID = r.RelativeID,
					FirstName = r.FirstName,
					LastName = r.LastName,
					Thumbnail = r.Thumbnail,
					LastModifiedOn = r.LastModifiedOn,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<Relative>> GetRelativeKeysAsync(int skip, int take, DataRequest<Relative> request)
		{
			IQueryable<Relative> items = GetRelatives(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Relative
				{
					RelativeID = r.RelativeID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<Relative> GetRelatives(DataRequest<Relative> request, bool skipSorting = false)
		{
			IQueryable<Relative> items = _learnDataSource.Relatives;

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
							? ((IOrderedQueryable<Relative>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<Relative>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetRelativesCountAsync(DataRequest<Relative> request)
		{
			return await GetRelatives(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateRelativeAsync(Relative entity)
		{
			if (entity.RelativeID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.RelativeID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteRelativesAsync(params Relative[] entities)
		{
			return await _learnDataSource.Relatives
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
