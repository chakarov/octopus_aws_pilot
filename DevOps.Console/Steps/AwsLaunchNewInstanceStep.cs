namespace DevOps.Console.Steps
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using Amazon.EC2;
	using Amazon.EC2.Model;
	using Aws.Integration;

	public class AwsLaunchNewInstanceStep : IDeployStep
	{
		private readonly AwsClient client;
		private readonly InstanceDeploy instance;

		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		public AwsLaunchNewInstanceStep(AwsClient client, InstanceDeploy instance)
		{
			this.client = client;
			this.instance = instance;

			Name = "Launch new instance in Aws";
		}

		public void Run()
		{
			var amiId = ConfigurationManager.AppSettings["aws-imiId"];
			var keyPair = ConfigurationManager.AppSettings["aws-keyPair"];
			var groupSecurity = ConfigurationManager.AppSettings["aws-groupSecurity"];

			var groups = new List<string>() { groupSecurity };

			var launchRequest = new RunInstancesRequest
			{
				ImageId = amiId,
				InstanceType = InstanceType.M3Medium,
				MinCount = 1,
				MaxCount = 1,
				KeyName = keyPair,
				SecurityGroupIds = groups,
				
			};

			try
			{
				var launchResponse = client.Ec2Client.RunInstances(launchRequest);

				var instances = launchResponse.Reservation.Instances;
				
				foreach (var item in instances)
				{
					instance.InstanceId = item.InstanceId;
					instance.Instance = item;
				}

				FinishedSuccessfully = true;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}
		}
	}
}
