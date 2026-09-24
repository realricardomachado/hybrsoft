using Hybrsoft.Enums;
using System;

namespace Hybrsoft.UI.Windows.Services
{
	public interface IAuthorizationService
	{
		bool HasSecurityAdministrationPermission { get; }

		bool CanAccessViewModel(Type viewModel);
		Type GetDefaultAccessibleViewModel();
		bool HasPermission(Permissions permission);
	}
}
