namespace DevOps.Console.Steps
{
	using System;
	using Octopus.Client.Exceptions;
	using OctopusDeploy.Integration;

	public class OctopusManagerProjectStep : IDeployStep
	{
		private readonly OctopusConnector connector;
		private readonly string branch;

		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		public OctopusManagerProjectStep(OctopusConnector connector, string branch)
		{
			this.connector = connector;
			this.branch = branch;
			Name = string.Format("Creating project in Octopus for branch {0}.", branch);
		}

		public void Run()
		{
			try
			{
				var projectGroupId = connector.repository.ProjectGroups.FindByName("ProjectGroupDestiny");

				var baseProject = connector.repository.Projects.FindByName("ProjectToClone");

				var urlClone = string.Format("/api/projects?clone={0}", baseProject.Id);

				baseProject.Id = "";
				baseProject.Name = branch;
				baseProject.Description = "";
				baseProject.ProjectGroupId = projectGroupId.Id;

				var project = connector.repository.Projects.FindByName(branch);

				if (project != null)
				{
					connector.repository.Projects.Delete(project);
				}

				connector.repository.Client.Post(urlClone, baseProject);

				FinishedSuccessfully = true;
			}
			catch (OctopusValidationException ex)
			{
				if (ex.Message.Contains("A project with this name already exists"))
					FinishedSuccessfully = true;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
				FinishedSuccessfully = false;
			}

			
		}
	}
}
