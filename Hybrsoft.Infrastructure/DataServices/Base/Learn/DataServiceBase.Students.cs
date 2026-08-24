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
		public async Task<Student> GetStudentAsync(long id)
		{
			return await _learnDataSource.Students.Where(r => r.StudentID == id).FirstOrDefaultAsync();
		}

		public async Task<IList<Student>> GetStudentsAsync(int skip, int take, DataRequest<Student> request)
		{
			IQueryable<Student> items = GetStudents(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Student
				{
					StudentID = r.StudentID,
					FirstName = r.FirstName,
					LastName = r.LastName,
					Email = r.Email,
					Thumbnail = r.Thumbnail,
					LastModifiedOn = r.LastModifiedOn,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<Student>> GetStudentKeysAsync(int skip, int take, DataRequest<Student> request)
		{
			IQueryable<Student> items = GetStudents(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Student
				{
					StudentID = r.StudentID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<Student> GetStudents(DataRequest<Student> request, bool skipSorting = false)
		{
			IQueryable<Student> items = _learnDataSource.Students;

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
							? ((IOrderedQueryable<Student>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<Student>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetStudentsCountAsync(DataRequest<Student> request)
		{
			return await GetStudents(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateStudentAsync(Student entity)
		{
			if (entity.StudentID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.StudentID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteStudentsAsync(params Student[] entities)
		{
			return await _learnDataSource.Students
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
