using Hybrsoft.EnterpriseManager.Services.DataServiceFactory;
using Hybrsoft.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hybrsoft.UI.Windows.Services
{
	public class LogEmbeddingService(IDataServiceFactory dataServiceFactory,
		IEmbeddingService embeddingService) : IEmbeddingTables
	{
		private readonly IDataServiceFactory _dataServiceFactory = dataServiceFactory;
		private readonly IEmbeddingService _embeddingService = embeddingService;

		public async Task PopulateMissingEmbeddingsAsync()
		{
			if (!_embeddingService.IsConfigured) return;

			using var dataSource = _dataServiceFactory.CreateDataService();
			var items = await dataSource.GetAppLogsWithMissingEmbeddingsAsync();

			const int batchSize = 50;
			for (int i = 0; i < items.Count; i += batchSize)
			{
				var batchItems = items.Skip(i).Take(batchSize).ToList();

				var terms = new List<string>();
				var newEmbeddings = new List<AppLogEmbedding>();

				foreach (var item in batchItems)
				{
					terms.Add(item.SearchTerms);

					var embedding = item.AppLogEmbeddings?.FirstOrDefault()
						?? new AppLogEmbedding { AppLogID = item.AppLogID };

					if (embedding.Embedding.IsNull || embedding.Embedding.Length == 0)
					{
						newEmbeddings.Add(embedding);
					}
				}

				if (terms.Count == 0) continue;

				// Generate embeddings in batch
				var generatedEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(terms);

				// Applies embeddings back to the objects
				for (int j = 0; j < generatedEmbeddings.Count; j++)
				{
					newEmbeddings[j].Embedding = generatedEmbeddings[j];
				}

				await dataSource.UpdateAppLogEmbeddingsAsync(newEmbeddings);
			}
		}
	}
}
