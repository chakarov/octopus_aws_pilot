namespace DevOps.Console.Steps
{
	using System;
	using Amazon.EC2.Model;
	using Aws.Integration;

	public class AwsAssociateElasticIpStep : IDeployStep
	{
		private readonly AwsClient client;
		private readonly Address ipAddress;
		private readonly InstanceDeploy instance;
		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		public AwsAssociateElasticIpStep(AwsClient client, Address ipAddress, InstanceDeploy instance)
		{
			this.client = client;
			this.ipAddress = ipAddress;
			this.instance = instance;
			Name = "Associating elastic ip.";
		}

		public void Run()
		{
			try
			{
				var associateRequest = new AssociateAddressRequest
				{
					AllocationId = ipAddress.AllocationId,
					InstanceId = instance.InstanceId
				};

				client.Ec2Client.AssociateAddress(associateRequest);

				instance.Address = ipAddress;

				instance.Ip = ipAddress.PublicIp;

				instance.Port = "10933";

				FinishedSuccessfully = true;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}
		}
	}
}
