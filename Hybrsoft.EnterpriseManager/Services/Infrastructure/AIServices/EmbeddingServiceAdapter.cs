using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;
using Microsoft.Data.SqlTypes;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hybrsoft.EnterpriseManager.Services
{
	public sealed partial class EmbeddingServiceAdapter : IEmbeddingService, IDisposable
	{
		private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
		private readonly string _logSourceName;

		private bool _isConfigured;
		public bool IsConfigured => _isConfigured;
		public int EmbeddingDimension { get; }

		public EmbeddingServiceAdapter(
			IEmbeddingGenerator<string, Embedding<float>> generator,
			int embeddingDimension,
			string logSourceName)
		{
			_generator = generator;
			EmbeddingDimension = embeddingDimension;
			_logSourceName = logSourceName;
			_isConfigured = generator != null;
		}

		public async Task<SqlVector<float>> GenerateEmbeddingAsync(string input)
		{
			if (!IsConfigured)
				return CreateVectorNull();

			if (string.IsNullOrWhiteSpace(input))
				throw new ArgumentException("Text cannot be empty.", nameof(input));

			try
			{
				GeneratedEmbeddings<Embedding<float>> result = await _generator.GenerateAsync((string[])[input]);
				return new SqlVector<float>(result[0].Vector.ToArray());
			}
			catch (Exception ex)
			{
				await HandleFailureAsync("Generate Embedding", ex);
				return CreateVectorNull();
			}
		}

		public async Task<IReadOnlyList<SqlVector<float>>> GenerateEmbeddingsAsync(IEnumerable<string> inputs)
		{
			ArgumentNullException.ThrowIfNull(inputs);

			if (!IsConfigured)
				return (SqlVector<float>[])[.. Enumerable.Repeat(CreateVectorNull(), inputs.Count())];

			try
			{
				List<string> inputList = [.. inputs];
				GeneratedEmbeddings<Embedding<float>> result = await _generator.GenerateAsync(inputList);

				return (SqlVector<float>[])[.. result.Select(embedding => new SqlVector<float>(embedding.Vector.ToArray()))];
			}
			catch (Exception ex)
			{
				await HandleFailureAsync("Generate Embeddings", ex);
				return (SqlVector<float>[])[.. Enumerable.Repeat(CreateVectorNull(), inputs.Count())];
			}
		}

		private SqlVector<float> CreateVectorNull()
		{
			return SqlVector<float>.CreateNull(EmbeddingDimension);
		}

		private async Task HandleFailureAsync(string operation, Exception ex)
		{
			DisableService();

			ISettingsService settingsService = ServiceLocator.Current.GetService<ISettingsService>();
			settingsService.UseSemanticSearch = false;
			settingsService.IsSemanticSearchEnabled = false;

			ILogService logService = ServiceLocator.Current.GetService<ILogService>();
			await logService.WriteAsync(LogType.Error, _logSourceName, operation, ex);
		}

		private void DisableService()
		{
			_isConfigured = false;
		}

		public void Dispose()
		{
			(_generator as IDisposable)?.Dispose();
		}
	}
}
