namespace DevOps.Console
{
	using Amazon.EC2.Model;
	using Octopus.Client.Model;

	public class InstanceDeploy
	{
		public string InstanceId { get; set; }

		public string State { get; set; }

		private string ip { get; set; }

		public string Ip
		{
			get
			{
				return Address != null ? Address.PublicIp : ip;
			}
			set { ip = value; }
		}

		public string Port { get; set; }

		public string Host
		{
			get { return string.Format("{0}:{1}", ip, Port); }
		}

		public string Branch { get; set; }

		public string Release { get; set; }

		public string CurrentStatus { get; set; }

		public string PreviousStatus { get; set; }

		public string ReleaseId { get; set; }

		public string DeploymentId { get; set; }

		public Instance Instance { get; set; }

		public Address Address { get; set; }

		public MachineResource Machine { get; set; }
	}
}
