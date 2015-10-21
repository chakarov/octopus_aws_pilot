namespace DevOps.Aws.Integration
{
	using System.Configuration;
	using System.Linq;
	using Amazon;
	using Amazon.EC2;
	using Amazon.EC2.Model;

	public class AwsClient
	{
		public readonly AmazonEC2Client Ec2Client;
		
		public AwsClient()
		{
			Ec2Client = new AmazonEC2Client(ConfigurationManager.AppSettings["AWSAccessKey"], ConfigurationManager.AppSettings["AWSSecretKey"], RegionEndpoint.USEast1);
		}

		public Address GetIpAvailable()
		{
			var daRequest = new DescribeAddressesRequest();
			var daResponse = Ec2Client.DescribeAddresses(daRequest);
			return daResponse.Addresses.FirstOrDefault(i => i.InstanceId == null);
		}
	}
}