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
		public async Task<LostAndFound> GetLostAndFoundAsync(long id)
		{
			return await _learnDataSource.LostAndFound
				.Where(r => r.LostAndFoundID == id)
				.Include(r => r.StudentBelonging)
				.ThenInclude(r => r.Student)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<LostAndFound>> GetLostAndFoundAsync(int skip, int take, DataRequest<LostAndFound> request)
		{
			IQueryable<LostAndFound> items = GetLostAndFound(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new LostAndFound
				{
					LostAndFoundID = r.LostAndFoundID,
					DisplayName = r.DisplayName,
					Thumbnail = r.Thumbnail,
					Status = r.Status,
					LastModifiedOn = r.LastModifiedOn
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		public async Task<IList<LostAndFound>> GetLostAndFoundKeysAsync(int skip, int take, DataRequest<LostAndFound> request)
		{
			IQueryable<LostAndFound> items = GetLostAndFound(request);

			// Execute
			var records = await items.Skip(skip).Take(take)
				.Select(r => new LostAndFound
				{
					LostAndFoundID = r.LostAndFoundID,
				})
				.AsNoTracking()
				.ToListAsync();

			return records;
		}

		private IQueryable<LostAndFound> GetLostAndFound(DataRequest<LostAndFound> request, bool skipSorting = false)
		{
			IQueryable<LostAndFound> items = _learnDataSource.LostAndFound;

			// Query
			if (!string.IsNullOrWhiteSpace(request.Query))
			{
				items = items.Where(r => EF.Functions.Like(r.SearchTerms, "%" + request.Query + "%"));
			}
			// Query Semantic
			if (request.UseSemanticSearch && !request.QueryEmbedding.IsNull)
			{
				var likeQuery = items.Select(f => new { Id = f.LostAndFoundID, Score = 1.0 });

				var vectorQuery = from emb in _learnDataSource.LostAndFoundEmbeddings
								  let score = EF.Functions.VectorDistance("cosine", emb.Embedding, request.QueryEmbedding)
								  where score < 0.7
								  select new { Id = emb.LostAndFoundID, Score = score };

				// Combine and keep best score per id to avoid duplicates
				var combinedBest = likeQuery.Union(vectorQuery)
					.GroupBy(x => x.Id)
					.Select(g => new { Id = g.Key, Score = g.Min(x => x.Score) });

				// Join back to entity, order by score (subquery only selected id and score)
				items = combinedBest
					.Join(_learnDataSource.LostAndFound, c => c.Id, e => e.LostAndFoundID, (c, e) => new { e, c.Score })
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
							? ((IOrderedQueryable<LostAndFound>)items).ThenByDescending(keySelector)
							: ((IOrderedQueryable<LostAndFound>)items).ThenBy(keySelector);
					}
				}
			}

			return items;
		}

		public async Task<int> GetLostAndFoundCountAsync(DataRequest<LostAndFound> request)
		{
			return await GetLostAndFound(request, true)
				.AsNoTracking()
				.CountAsync();
		}

		public async Task<int> UpdateLostAndFoundAsync(LostAndFound entity)
		{
			if (entity.LostAndFoundID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.LostAndFoundID = UIDGenerator.Next();
				entity.CreatedOn = DateTimeOffset.Now;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			entity.LastModifiedOn = DateTimeOffset.Now;
			entity.SearchTerms = entity.BuildSearchTerms();
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> DeleteLostAndFoundAsync(params LostAndFound[] entities)
		{
			return await _learnDataSource.LostAndFound
				.Where(r => entities.Contains(r))
				.ExecuteDeleteAsync();
		}


		public async Task<LostAndFoundEmbedding> GetLostAndFoundEmbeddingAsync(long id)
		{
			return await _learnDataSource.LostAndFoundEmbeddings
				.Where(e => e.LostAndFoundEmbeddingID == id)
				.FirstOrDefaultAsync();
		}

		public async Task<IList<LostAndFound>> GetLostAndFoundWithMissingEmbeddingsAsync()
		{
			string query = @$"
				SELECT item.*
				FROM [Learn].[LostAndFound] item
					LEFT JOIN [Learn].[LostAndFoundEmbedding] emb
						ON item.[LostAndFoundID] = emb.[LostAndFoundID]
				WHERE emb.[LostAndFoundEmbeddingID] IS NULL OR emb.[Embedding] IS NULL";

			return await _learnDataSource.LostAndFound
				.FromSqlRaw(query)
				.Include(item => item.LostAndFoundEmbeddings)
				.ToListAsync();
		}

		public async Task<int> UpdateLostAndFoundEmbeddingAsync(LostAndFoundEmbedding entity)
		{
			if (entity.LostAndFoundEmbeddingID > 0)
			{
				_learnDataSource.Entry(entity).State = EntityState.Modified;
			}
			else
			{
				entity.LostAndFoundEmbeddingID = entity.LostAndFoundID;
				_learnDataSource.Entry(entity).State = EntityState.Added;
			}
			return await _learnDataSource.SaveChangesAsync();
		}

		public async Task<int> UpdateLostAndFoundEmbeddingsAsync(IEnumerable<LostAndFoundEmbedding> entities)
		{
			foreach (var entity in entities)
			{
				if (entity.LostAndFoundEmbeddingID > 0)
				{
					_learnDataSource.Entry(entity).State = EntityState.Modified;
				}
				else
				{
					entity.LostAndFoundEmbeddingID = entity.LostAndFoundID;
					_learnDataSource.Entry(entity).State = EntityState.Added;
				}
			}
			return await _learnDataSource.SaveChangesAsync();
		}
	}
}
