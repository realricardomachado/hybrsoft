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
		public async Task<Subscription> GetSubscriptionAsync(long id)
		{
			return await _universalDataSource.Subscriptions
				.Where(r => r.SubscriptionID == id)
				.Include(r => r.Company)
					.ThenInclude(r => r.Country)
				.Include(r => r.User)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<Subscription>> GetSubscriptionsAsync(int skip, int take, DataRequest<Subscription> request)
		{
			IQueryable<Subscription> items = GetSubscriptions(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Subscription
				{
					SubscriptionID = r.SubscriptionID,
					LicenseKey = r.LicenseKey,
					Status = r.Status,
					ExpirationDate = r.ExpirationDate
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<Subscription>> GetSubscriptionKeysAsync(int skip, int take, DataRequest<Subscription> request)
		{
			IQueryable<Subscription> items = GetSubscriptions(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Subscription
				{
					SubscriptionID = r.SubscriptionID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<Subscription> GetSubscriptions(DataRequest<Subscription> request, bool skipSorting = false)
		{
			IQueryable<Subscription> items = _universalDataSource.Subscriptions;

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
							? ((IOrderedQueryable<Subscription>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<Subscription>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetSubscriptionsCountAsync(DataRequest<Subscription> request)
		{
			return await GetSubscriptions(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateSubscriptionAsync(Subscription entity)
		{
			if (entity.SubscriptionID > 0)
			{
				_universalDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.SubscriptionID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				long targetID = entity.Type switch
				{
					SubscriptionType.Enterprise => entity.CompanyID.Value,
					SubscriptionType.Individual => entity.UserID.Value,
					_ => 0
				};
				entity.LicenseKey = LicenseGenerator.GenerateLicenseKey(entity.SubscriptionID, entity.DurationDays, entity.CreatedOn, targetID);
				_universalDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _universalDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteSubscriptionsAsync(params Subscription[] entities)
		{
			return await _universalDataSource.Subscriptions
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
