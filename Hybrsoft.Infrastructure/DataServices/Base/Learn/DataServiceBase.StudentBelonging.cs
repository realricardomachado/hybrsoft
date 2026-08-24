using Hybrsoft.Enums;
using Hybrsoft.Infrastructure.Common;
using Hybrsoft.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hybrsoft.Infrastructure.DataServices.Base
{
	partial class DataServiceBase
	{
		public async Task<StudentBelonging> GetStudentBelongingAsync(long id)
		{
			return await _learnDataSource.StudentBelongings
				.Where(r => r.StudentBelongingID == id)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<StudentBelonging>> GetStudentBelongingsAsync(int skip, int take, DataRequest<StudentBelonging> request)
		{
			IQueryable<StudentBelonging> items = GetStudentBelongings(request);
			bool includeStudent = request.Includes
				.Any(i => i.Body.ToString().Contains(nameof(StudentBelonging.Student)));
			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new StudentBelonging
				{
					StudentBelongingID = r.StudentBelongingID,
					StudentID = r.StudentID,
					DisplayName = r.DisplayName,
					Thumbnail = r.Thumbnail,
					Student = includeStudent
					? new Student
					{
						StudentID = r.Student.StudentID,
						FirstName = r.Student.FirstName,
						LastName = r.Student.LastName,
						Thumbnail = r.Student.Thumbnail,
						CreatedOn = r.CreatedOn
					}
					: null
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<StudentBelonging>> GetStudentBelongingKeysAsync(int skip, int take, DataRequest<StudentBelonging> request)
		{
			IQueryable<StudentBelonging> items = GetStudentBelongings(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new StudentBelonging
				{
					StudentBelongingID = r.StudentBelongingID
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<StudentBelonging> GetStudentBelongings(DataRequest<StudentBelonging> request, bool skipSorting = false)
		{
			IQueryable<StudentBelonging> items = _learnDataSource.StudentBelongings;

			// Query
			if (!string.IsNullOrWhiteSpace(request.Query))
			{
				if (request.Includes.Any(expr => expr.Body is MemberExpression member
										&& member.Member.Name == nameof(StudentBelonging.Student)))
				{
					items = items.Where(r => EF.Functions.Like(r.SearchTermsStudent, "%" + request.Query + "%"));
				}
				else
				{
					items = items.Where(r => EF.Functions.Like(r.SearchTerms, "%" + request.Query + "%"));
				}
			}
			// Query Semantic
			if (request.UseSemanticSearch && !request.QueryEmbedding.IsNull)
			{
				var likeQuery = items.Select(f => new { Id = f.StudentBelongingID, Score = 1.0 });

				var vectorQuery = from emb in _learnDataSource.StudentBelongingEmbeddings
								  let score = EF.Functions.VectorDistance("cosine", emb.Embedding, request.QueryEmbedding)
								  where score < 0.7
								  select new { Id = emb.StudentBelongingID, Score = score };

				// Combine and keep best score per id to avoid duplicates
				var combinedBest = likeQuery.Union(vectorQuery)
					.GroupBy(x => x.Id)
					.Select(g => new { Id = g.Key, Score = g.Min(x => x.Score) });

				// Join back to entity, order by score (subquery only selected id and score)
				items = combinedBest
					.Join(_learnDataSource.StudentBelongings, c => c.Id, e => e.StudentBelongingID, (c, e) => new { e, c.Score })
					.OrderBy(x => x.Score)
					.Select(x => x.e);

				skipSorting = true;
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
							? ((IOrderedQueryable<StudentBelonging>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<StudentBelonging>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetStudentBelongingsCountAsync(DataRequest<StudentBelonging> request)
		{
			return await GetStudentBelongings(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateStudentBelongingAsync(StudentBelonging entity)
		{
			if (entity.StudentBelongingID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.StudentBelongingID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			entity.SearchTermsStudent = entity.BuildSearchTermsStudent();
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteStudentBelongingsAsync(params StudentBelonging[] entities)
		{
			return await _learnDataSource.StudentBelongings
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}


		public async Task<StudentBelongingEmbedding> GetStudentBelongingEmbeddingAsync(long id)
		{
			return await _learnDataSource.StudentBelongingEmbeddings
				.Where(r => r.StudentBelongingEmbeddingID == id)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<StudentBelonging>> GetStudentBelongingWithMissingEmbeddingsAsync()
		{
			string query = @$"
				SELECT item.*
				FROM [Learn].[StudentBelonging] item
					LEFT JOIN [Learn].[StudentBelongingEmbedding] emb
						ON item.[StudentBelongingID] = emb.[StudentBelongingID]
				WHERE emb.[StudentBelongingEmbeddingID] IS NULL OR emb.[Embedding] IS NULL";

			return await _learnDataSource.StudentBelongings
				.FromSqlRaw(query)
				.Include(item => item.StudentBelongingEmbeddings)
				.ToListAsync();
		}

		public async Task<int> UpdateStudentBelongingEmbeddingAsync(StudentBelongingEmbedding entity)
		{
			if (entity.StudentBelongingEmbeddingID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.StudentBelongingEmbeddingID = entity.StudentBelongingID;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> UpdateStudentBelongingEmbeddingsAsync(IEnumerable<StudentBelongingEmbedding> entities)
		{
			foreach (var entity in entities)
			{
				if (entity.StudentBelongingEmbeddingID > 0)
				{
					_learnDataSource.Entry(entity).State = EntityState.Modified;
				}
				else
				{
					entity.StudentBelongingEmbeddingID = entity.StudentBelongingID;
					_learnDataSource.Entry(entity).State = EntityState.Added;
				}
			}
			return await _learnDataSource.SaveChangesAsync();
		}
	}
}
