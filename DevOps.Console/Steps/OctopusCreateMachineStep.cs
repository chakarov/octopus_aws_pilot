namespace DevOps.Console.Steps
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Threading;
	using Octopus.Client.Model;
	using OctopusDeploy.Integration;

	public class OctopusCreateMachineStep : IDeployStep
	{
		private readonly OctopusConnector client;
		private readonly InstanceDeploy instanceDeploy;
		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		public Dictionary<string, string> IpsMap = new Dictionary<string, string>()
		{
			//{"aws ip", "route ip to aws"},
		};

		public OctopusCreateMachineStep(OctopusConnector client, InstanceDeploy instanceDeploy)
		{
			this.client = client;
			this.instanceDeploy = instanceDeploy;
			Name = "Creating tentacle machine.";
		}

		private string GetLocalIp()
		{
			var local = Convert.ToBoolean(ConfigurationManager.AppSettings["testlocal"]);

			if (!local) return instanceDeploy.Host;

			return IpsMap.ContainsKey(instanceDeploy.Host) ? IpsMap[instanceDeploy.Host] : instanceDeploy.Host;
		}

		public void Run()
		{

			try
			{
				MachineResource machine = null;

				var host = GetLocalIp().Split(':');

				var limit = 20;

				while (limit > 0)
				{
					try
					{
						machine = client.repository.Machines.Discover(host[0], Int32.Parse(host[1]), DiscoverableEndpointType.TentaclePassive);

						if (machine != null)
							break;
					}
					catch (Exception ex)
					{
						Error = ex.Message;
					}

					Thread.Sleep(15000);
					limit --;
				}

				FinishedSuccessfully = true;

				if (machine == null)
					FinishedSuccessfully = false;
				else
				{
					var machineExist = client.repository.Machines.FindByName(instanceDeploy.Ip);

					if (machineExist != null)
					{
						if (machineExist.IsDisabled)
						{
							machineExist.IsDisabled = false;

							client.repository.Machines.Modify(machineExist);
						}

						instanceDeploy.Machine = machineExist;
					}
					else
					{
						var environments = client.repository.Environments.FindByName("environment");

						machine.EnvironmentIds = new ReferenceCollection(environments.Id);

						machine.Roles = new ReferenceCollection();

						machine.Name = instanceDeploy.Ip;

						var newMachine = client.repository.Machines.Create(machine);

						instanceDeploy.Machine = newMachine;
					}

					var task = client.repository.Tasks.ExecuteHealthCheck(machineIds: new[] { instanceDeploy.Machine.Id });

					limit = 10;

					while (limit > 0)
					{
						var runningTask = client.repository.Tasks.FindOne(t => t.Id == task.Id);

						if (runningTask.FinishedSuccessfully || runningTask.IsCompleted)
						{
							break;
						}

						Thread.Sleep(60000);
						limit --;
					}
				}
			}
			catch (Exception ex)
			{
				FinishedSuccessfully = false;
				Error = ex.Message;
			}

			

			

		}
	}
}
