namespace DevOps.Console.Steps
{
	using System.Collections.Generic;
	using System.Threading;
	using Amazon.EC2.Model;
	using Aws.Integration;

	public class AwsCheckTheStateStep : IDeployStep
	{
		private readonly AwsClient awsClient;
		private readonly InstanceDeploy instance;
		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		public AwsCheckTheStateStep(AwsClient awsClient, InstanceDeploy instance)
		{
			this.awsClient = awsClient;
			this.instance = instance;
			Name = "Checking instance state.";
		}

		public void Run()
		{
			var instanceRequest = new DescribeInstancesRequest
			{
				InstanceIds = new List<string>()
				{
					instance.InstanceId
				}
			};

			var limit = 600;

			DescribeInstancesResponse response = null;

			while (limit > 0)
			{
				response = awsClient.Ec2Client.DescribeInstances(instanceRequest);

				if (response.Reservations[0].Instances[0].State.Code == 16)
					break;

				limit --;

				Thread.Sleep(1000);
			}

			if (response != null && response.Reservations[0].Instances[0].State.Code == 16)
				FinishedSuccessfully = true;
			else
			{
				Error = string.Format("Instance not launched yet. Last state: {0}.", response.Reservations[0].Instances[0].State.Name);
			}
		}
	}
}
