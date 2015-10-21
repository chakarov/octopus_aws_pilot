namespace DevOps.Console
{
	using System;
	using DevOps.Aws.Integration;
	using DevOps.Console.Steps;
	using DevOps.Nuget.Integration;
	using DevOps.OctopusDeploy.Integration;

	class Program
	{
		static int Main(string[] args)
		{
			Console.WriteLine("Deploy running...");

			try
			{
				var instanceDeploy = new InstanceDeploy()
				{
					Branch = "",
					Release = ""
				};

				if (args.Length > 0)
				{
					instanceDeploy.Branch = args[0];
					instanceDeploy.Release = args[1];
				}

				var deployStatus = true;
				var error = "";

				var awsClient = new AwsClient();
				var nugetClient = new NugetClient();
				
				var stepsUp = new IDeployStep[]
				{
					new OctopusManagerProjectStep(OctopusConnector.GetOrCreate(), instanceDeploy.Branch), 
					new AwsLaunchNewInstanceStep(awsClient, instanceDeploy), 
					new AwsCheckTheStateStep(awsClient, instanceDeploy), 
					new AwsAssociateElasticIpStep(awsClient, awsClient.GetIpAvailable(), instanceDeploy),
					new OctopusCreateMachineStep(OctopusConnector.GetOrCreate(), instanceDeploy), 
					new OctopusCreateReleaseStep(OctopusConnector.GetOrCreate(), nugetClient, instanceDeploy), 
					new OctopusDeploytoAwsStep(OctopusConnector.GetOrCreate(), instanceDeploy),
				};

				var stepsDown = new IDeployStep[]
				{
					new AwsTerminateInstanceStep(awsClient, instanceDeploy), 
				};

				foreach (var deployStep in stepsUp)
				{
					Console.WriteLine("--Step {0}", deployStep.Name);

					deployStep.Run();

					deployStatus = deployStep.FinishedSuccessfully;

					if (deployStep.FinishedSuccessfully)
						Console.WriteLine("----Finished Successfully!");
					else
					{
						error = deployStep.Error;
						Console.WriteLine("----Finished with Error {0}", deployStep.Error);
						break;
					}
				}

				foreach (var step in stepsDown)
				{
					Console.WriteLine("--Step {0}", step.Name);

					step.Run();

					if (step.FinishedSuccessfully)
						Console.WriteLine("----Finished Successfully!");
					else
					{
						Console.WriteLine("----Finished with Error {0}", step.Error);
					}
				}
				
				var slackNotification = new SlackNotificationStep(instanceDeploy, deployStatus, error);
				slackNotification.Run();
				
				Console.WriteLine("");
				Console.WriteLine("Deploy:");
				Console.WriteLine("Branch: {0}", instanceDeploy.Branch);
				Console.WriteLine("Release: {0}, Id: {1}", instanceDeploy.Release, instanceDeploy.ReleaseId);
				Console.WriteLine("Ip: {0}", instanceDeploy.Ip);
				Console.WriteLine("Instance: {0}", instanceDeploy.InstanceId);
				Console.WriteLine("Previous Status: {0}", instanceDeploy.PreviousStatus);
				Console.WriteLine("Last Status: {0}", instanceDeploy.CurrentStatus);

				Console.WriteLine("Deploy finished!");

				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: ");
				Console.WriteLine(ex.Message);

				return -1;
			}
		}
	}
}
