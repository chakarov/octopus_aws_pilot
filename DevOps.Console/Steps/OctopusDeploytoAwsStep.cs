namespace DevOps.Console.Steps
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Octopus.Client.Model;
	using OctopusDeploy.Integration;

	public class OctopusDeploytoAwsStep : IDeployStep
	{
		private readonly OctopusConnector client;
		private readonly InstanceDeploy instance;
		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		public OctopusDeploytoAwsStep(OctopusConnector client, InstanceDeploy instance)
		{
			this.client = client;
			this.instance = instance;
			Name = "Octopus Deploy running";
		}

		public void Run()
		{
			try
			{
				var deploymentResource = new DeploymentResource()
				{
					EnvironmentId = "EnvironmentsId",
					ReleaseId = instance.ReleaseId,
					SpecificMachineIds = new ReferenceCollection(instance.Machine.Id),
					Name = string.Format("Deploy To Aws {0} {1} {2}", instance.Ip, instance.Branch, instance.Release),
					FormValues = new Dictionary<string, string>()
					{
						
					}
				};
				

				var deployment = client.repository.Deployments.Create(deploymentResource);

				instance.DeploymentId = deployment.Id;

				var limit = 45;

				while (limit > 0)
				{
					var task = client.repository.Tasks.FindOne(t => t.Id == deployment.TaskId);
					if (task.FinishedSuccessfully)
					{
						FinishedSuccessfully = true;
					}

					if (task.IsCompleted)
					{
						break;
					}

					Thread.Sleep(60000);
					limit--;
				}
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

		}
	}
}
