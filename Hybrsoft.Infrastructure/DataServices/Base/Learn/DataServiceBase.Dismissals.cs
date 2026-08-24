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
		public async Task<IList<ClassroomStudent>> GetDismissibleStudentsAsync(int skip, int take, DataRequest<ClassroomStudent> request)
		{
			IQueryable<ClassroomStudent> items = GetClassroomStudentsForDismissal(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(cs => new ClassroomStudent
				{
					ClassroomStudentID = cs.ClassroomStudentID,
					ClassroomID = cs.ClassroomID,
					StudentID = cs.StudentID,
					Classroom = new Classroom
					{
						ClassroomID = cs.Classroom.ClassroomID,
						Name = cs.Classroom.Name
					},
					Student = new Student
					{
						StudentID = cs.Student.StudentID,
						FirstName = cs.Student.FirstName,
						LastName = cs.Student.LastName,
						Thumbnail = cs.Student.Thumbnail,
					},
				})
				.AsNoTracking()
				.ToListAsync();
			return records;
		}

		public async Task<IList<ClassroomStudent>> GetDismissibleStudentKeysAsync(int skip, int take, DataRequest<ClassroomStudent> request)
		{
			IQueryable<ClassroomStudent> items = GetClassroomStudentsForDismissal(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new ClassroomStudent
				{
					ClassroomID = r.ClassroomID,
					StudentID = r.StudentID,
				})
				.AsNoTracking()
				.ToListAsync();
			return records;
		}

		private IQueryable<ClassroomStudent> GetClassroomStudentsForDismissal(DataRequest<ClassroomStudent> request, bool skipSorting = false)
		{
			IQueryable<ClassroomStudent> items = _learnDataSource.ClassroomStudents;

			// Query
			if (!string.IsNullOrEmpty(request.Query))
			{
				items = items.Where(r => EF.Functions.Like(r.SearchTermsDismissibleStudent, "%" + request.Query + "%"));
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

		public async Task<int> GetDismissibleStudentsCountAsync(DataRequest<ClassroomStudent> request)
		{
			return await GetClassroomStudentsForDismissal(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<Dismissal> GetDismissalAsync(long id)
		{
			return await _learnDataSource.Dismissals
				.Where(r => r.DismissalID == id)
				.Include(r => r.Classroom)
				.ThenInclude(r => r.ScheduleType)
				.Include(r => r.Student)
				.Include(r => r.Relative)
				.ThenInclude(r => r.RelativeType)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<Dismissal>> GetDismissalsAsync(int skip, int take, DataRequest<Dismissal> request)
		{
			IQueryable<Dismissal> items = GetDismissals(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Dismissal
				{
					DismissalID = r.DismissalID,
					ClassroomID = r.ClassroomID,
					Classroom = new Classroom
					{
						ClassroomID = r.Classroom.ClassroomID,
						Name = r.Classroom.Name,
						ScheduleType = new ScheduleType()
					},
					StudentID = r.StudentID,
					Student = new Student
					{
						StudentID = r.Student.StudentID,
						FirstName = r.Student.FirstName,
						LastName = r.Student.LastName,
						Thumbnail = r.Student.Thumbnail
					},
					RelativeID = r.RelativeID,
					Relative = new Relative
					{
						RelativeID = r.Relative.RelativeID,
						FirstName = r.Relative.FirstName,
						LastName = r.Relative.LastName,
						Thumbnail = r.Relative.Thumbnail,
						RelativeType = new RelativeType()
					},
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<Dismissal>> GetDismissalKeysAsync(int skip, int take, DataRequest<Dismissal> request)
		{
			IQueryable<Dismissal> items = GetDismissals(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new Dismissal
				{
					DismissalID = r.DismissalID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<Dismissal> GetDismissals(DataRequest<Dismissal> request, bool skipSorting = false)
		{
			IQueryable<Dismissal> items = _learnDataSource.Dismissals;

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
							? ((IOrderedQueryable<Dismissal>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<Dismissal>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetDismissalsCountAsync(DataRequest<Dismissal> request)
		{
			return await GetDismissals(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateDismissalAsync(Dismissal entity)
		{
			if (entity.DismissalID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
				entity.DismissedOn = DateTimeOffset.Now;
			}
			else
			{
				entity.DismissalID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> ApproveDismissalsAsync(params Dismissal[] entities)
		{
			return await _learnDataSource.Dismissals
				.Where(r => entities.Contains(r))
				.ExecuteUpdateAsync(r => r.SetProperty(x => x.DismissedOn, DateTimeOffset.Now));
		}
	}
}
