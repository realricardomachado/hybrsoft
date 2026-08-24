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
		public async Task<Classroom> GetClassroomAsync(long id)
		{
			return await _learnDataSource.Classrooms
				.Where(r => r.ClassroomID == id)
				.Include(r => r.ScheduleType)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<Classroom>> GetClassroomsAsync(int skip, int take, DataRequest<Classroom> request)
		{
			IQueryable<Classroom> items = GetClassrooms(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Classroom
				{
					ClassroomID = r.ClassroomID,
					Name = r.Name,
					Year = r.Year,
					EducationLevel = r.EducationLevel,
					ScheduleType = r.ScheduleType,
					LastModifiedOn = r.LastModifiedOn,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<Classroom>> GetClassroomKeysAsync(int skip, int take, DataRequest<Classroom> request)
		{
			IQueryable<Classroom> items = GetClassrooms(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Classroom
				{
					ClassroomID = r.ClassroomID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<Classroom> GetClassrooms(DataRequest<Classroom> request, bool skipSorting = false)
		{
			IQueryable<Classroom> items = _learnDataSource.Classrooms;

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
							? ((IOrderedQueryable<Classroom>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<Classroom>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetClassroomsCountAsync(DataRequest<Classroom> request)
		{
			return await GetClassrooms(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateClassroomAsync(Classroom entity)
		{
			if (entity.ClassroomID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.ClassroomID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteClassroomsAsync(params Classroom[] entities)
		{
			return await _learnDataSource.Classrooms
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
