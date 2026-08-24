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
		public async Task<StudentRelative> GetStudentRelativeAsync(long id)
		{
			return await _learnDataSource.StudentRelatives
				.Where(r => r.StudentRelativeID == id)
				.Include(r => r.Relative)
				.ThenInclude(r => r.RelativeType)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<StudentRelative>> GetStudentRelativesAsync(int skip, int take, DataRequest<StudentRelative> request)
		{
			IQueryable<StudentRelative> items = GetStudentRelatives(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new StudentRelative
				{
					StudentRelativeID = r.StudentRelativeID,
					StudentID = r.StudentID,
					RelativeID = r.RelativeID,
					Relative = new Relative
					{
						RelativeID = r.Relative.RelativeID,
						FirstName = r.Relative.FirstName,
						LastName = r.Relative.LastName,
						Thumbnail = r.Relative.Thumbnail
					}
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<StudentRelative>> GetStudentRelativeKeysAsync(int skip, int take, DataRequest<StudentRelative> request)
		{
			IQueryable<StudentRelative> items = GetStudentRelatives(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new StudentRelative
				{
					StudentID = r.StudentID,
					RelativeID = r.RelativeID
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<StudentRelative> GetStudentRelatives(DataRequest<StudentRelative> request, bool skipSorting = false)
		{
			IQueryable<StudentRelative> items = _learnDataSource.StudentRelatives;

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
							? ((IOrderedQueryable<StudentRelative>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<StudentRelative>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<IList<long>> GetAddedRelativeKeysInStudentAsync(long parentID)
		{
			return await _learnDataSource.StudentRelatives
				.AsNoTracking()
				.Where(r => r.StudentID == parentID)
				.Select(r => r.RelativeID)
				.ToListAsync();
		}

		public async Task<int> GetStudentRelativesCountAsync(DataRequest<StudentRelative> request)
		{
			return await GetStudentRelatives(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateStudentRelativeAsync(StudentRelative entity)
		{
			if (entity.StudentRelativeID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.StudentRelativeID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteStudentRelativesAsync(params StudentRelative[] entities)
		{
			return await _learnDataSource.StudentRelatives
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
