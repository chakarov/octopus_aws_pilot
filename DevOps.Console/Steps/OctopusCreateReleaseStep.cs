namespace DevOps.Console.Steps
{
	using System;
	using System.Configuration;
	using System.Linq;
	using Nuget.Integration;
	using Octopus.Client.Model;
	using OctopusDeploy.Integration;

	public class OctopusCreateReleaseStep : IDeployStep
	{
		private readonly OctopusConnector client;
		private readonly NugetClient nugetClient;
		private readonly InstanceDeploy instance;

		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		public OctopusCreateReleaseStep(OctopusConnector client, NugetClient nugetClient, InstanceDeploy instance)
		{
			this.client = client;
			this.nugetClient = nugetClient;
			this.instance = instance;
			Name = "Creating Release.";
		}

		public void Run()
		{
			try
			{
				var project = client.repository.Projects.FindByName(instance.Branch);

				var releases = client.repository.Releases.FindMany(r => r.ProjectId == ConfigurationManager.AppSettings["octopus-base-release-projectId"]);

				var releaseBase = releases.FirstOrDefault();

				var release = client.repository.Releases.FindOne(r => r.Version == instance.Release && r.ProjectId == project.Id);

				if (release == null)
				{
					var releaseResource = new ReleaseResource(instance.Release, project.Id);

					releaseResource.SelectedPackages = releaseBase.SelectedPackages;

					foreach (var package in releaseResource.SelectedPackages)
					{
						package.Version = nugetClient.GetLastVersion(package.StepName, instance.Branch);
					}

					var newRelease = client.repository.Releases.Create(releaseResource);

					instance.ReleaseId = newRelease.Id;
				}
				else
					instance.ReleaseId = release.Id;

				FinishedSuccessfully = true;
			}
			catch (Exception ex)
			{
				FinishedSuccessfully = false;

				Error = ex.Message;
			}

			
		}
	}
}
