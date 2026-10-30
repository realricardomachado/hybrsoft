using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Hybrsoft.EnterpriseManager.Configuration
{
	public static class AppConfig
	{
		public static IConfiguration Configuration { get; private set; }
		public static ISettingsService SettingsService { get; private set; }

		public static void Initialize()
		{
			var configPath = Path.Combine(AppContext.BaseDirectory, "Configuration");

			Configuration = new ConfigurationBuilder()
				.SetBasePath(configPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			SettingsService = ServiceLocator.Current.GetService<ISettingsService>();
#if DEBUG
			SettingsService.Environment = EnvironmentType.Development;
#else
			SettingsService.Environment = EnvironmentType.Production;
#endif
		}

		public static bool IsDevelopment => SettingsService.Environment == EnvironmentType.Development;

		public static string ApiBaseUrl => Configuration["AppSettings:ApiBaseUrl" + (IsDevelopment ? "Dev" : "Prod")];

		public static string Azure_OpenAI_API_Endpoint => Configuration["AzureSettings:OPENAI_API_ENDPOINT"];
		public static string Azure_OpenAI_API_Key => Configuration["AzureSettings:OPENAI_API_KEY"];
		public static string Azure_OpenAI_Embedding_Model => Configuration["AzureSettings:OPENAI_EMBEDDING_MODEL"];
		public static int Azure_OpenAI_Embedding_Dimension => Configuration.GetValue<int>("AzureSettings:OPENAI_EMBEDDING_DIMENSION");

	}
}
