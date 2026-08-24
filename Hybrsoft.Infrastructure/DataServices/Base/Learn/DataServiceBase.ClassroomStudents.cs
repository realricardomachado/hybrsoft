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
		public async Task<ClassroomStudent> GetClassroomStudentAsync(long id)
		{
			return await _learnDataSource.ClassroomStudents
				.Where(r => r.ClassroomStudentID == id)
				.Include(r => r.Student)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<ClassroomStudent>> GetClassroomStudentsAsync(int skip, int take, DataRequest<ClassroomStudent> request)
		{
			IQueryable<ClassroomStudent> items = GetClassroomStudents(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new ClassroomStudent
				{
					ClassroomStudentID = r.ClassroomStudentID,
					ClassroomID = r.ClassroomID,
					StudentID = r.StudentID,
					Student = new Student
					{
						StudentID = r.Student.StudentID,
						FirstName = r.Student.FirstName,
						LastName = r.Student.LastName,
						Thumbnail = r.Student.Thumbnail
					}
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<ClassroomStudent>> GetClassroomStudentKeysAsync(int skip, int take, DataRequest<ClassroomStudent> request)
		{
			IQueryable<ClassroomStudent> items = GetClassroomStudents(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new ClassroomStudent
				{
					ClassroomID = r.ClassroomID,
					StudentID = r.StudentID
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<ClassroomStudent> GetClassroomStudents(DataRequest<ClassroomStudent> request, bool skipSorting = false)
		{
			IQueryable<ClassroomStudent> items = _learnDataSource.ClassroomStudents;

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
							? ((IOrderedQueryable<ClassroomStudent>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<ClassroomStudent>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<IList<long>> GetAddedStudentKeysInClassroomAsync(long parentID)
		{
			return await _learnDataSource.ClassroomStudents
				.AsNoTracking()
				.Where(r => r.ClassroomID == parentID)
				.Select(r => r.StudentID)
				.ToListAsync();
		}

		public async Task<int> GetClassroomStudentsCountAsync(DataRequest<ClassroomStudent> request)
		{
			return await GetClassroomStudents(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateClassroomStudentAsync(ClassroomStudent entity)
		{
			if (entity.ClassroomStudentID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.ClassroomStudentID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			entity.SearchTermsDismissibleStudent = entity.BuildSearchTermsDismissibleStudent();
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteClassroomStudentsAsync(params ClassroomStudent[] entities)
		{
			return await _learnDataSource.ClassroomStudents
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}
	}
}
