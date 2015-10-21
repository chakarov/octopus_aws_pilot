namespace DevOps.Nuget.Integration
{
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using NuGet;

	public class NugetClient
	{
		private readonly IPackageRepository repository;

		public readonly Dictionary<string, string> StepsPackagesMap = new Dictionary<string, string>()
		{
			//{"step name", "nuget package name"},
		};

		public NugetClient()
		{
			repository = PackageRepositoryFactory.Default.CreateRepository(ConfigurationManager.AppSettings["nuget-feed"]);
		}

		public string GetLastVersion(string step, string branch)
		{
			if (!StepsPackagesMap.ContainsKey(step))
				return string.Empty;

			branch = branch.Contains("-") ? branch.ToUpper() : branch.ToLower();

			var packages = repository.FindPackagesById(string.Format("{0}{1}", StepsPackagesMap[step], branch)).ToList();

			var firstOrDefault = packages.FirstOrDefault(p => p.IsLatestVersion);

			return firstOrDefault != null ? firstOrDefault.Version.ToString() : string.Empty;
		}
	}
}
