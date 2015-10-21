namespace DevOps.Console.Steps
{
	using System.Collections.Generic;
	using Amazon.EC2;
	using Amazon.EC2.Model;
	using Aws.Integration;

	public class AwsTerminateInstanceStep : IDeployStep
	{
		private readonly AwsClient awsClient;
		private readonly InstanceDeploy instance;

		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		public AwsTerminateInstanceStep(AwsClient awsClient, InstanceDeploy instance)
		{
			this.awsClient = awsClient;
			this.instance = instance;
			Name = "Stopping aws instance.";
		}

		public void Run()
		{
			var request = new TerminateInstancesRequest
			{
				InstanceIds = new List<string>()
				{
					instance.InstanceId
				}
			};

			try
			{
				var responseTerminate = awsClient.Ec2Client.TerminateInstances(request);
				foreach (var item in responseTerminate.TerminatingInstances)
				{
					instance.CurrentStatus = item.CurrentState.Code.ToString();
					instance.PreviousStatus = item.PreviousState.Code.ToString();
				}

				if (instance.CurrentStatus == "32")
					FinishedSuccessfully = true;
			}
			catch (AmazonEC2Exception ex)
			{
				Error = "InvalidInstanceID.NotFound" == ex.ErrorCode ? "Instance does not exist." : ex.Message;
			}
		}
	}
}
