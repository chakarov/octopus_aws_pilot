namespace DevOps.OctopusDeploy.Integration
{
	using System.Configuration;
	using Octopus.Client;
	using Octopus.Client.Model;

	public class OctopusConnector
	{
		public static OctopusConnector connector = null;

		private readonly string Server = "";
		private readonly string ApiKey = "";

		private readonly string User = "";
		private readonly string Pass = "";

		public readonly OctopusServerEndpoint endpoint;
		public readonly OctopusRepository repository;

		private OctopusConnector()
		{
			User = ConfigurationManager.AppSettings["octopus-user"];
			Pass = ConfigurationManager.AppSettings["octopus-pass"];
			Server = ConfigurationManager.AppSettings["octopus-server"];
			ApiKey = ConfigurationManager.AppSettings["octopus-key"];

			endpoint = new OctopusServerEndpoint(Server, ApiKey);
			repository = new OctopusRepository(endpoint);
		}

		public void Connect()
		{
			repository.Users.SignIn(new LoginCommand { Username = User, Password = Pass });
		}

		public static OctopusConnector GetOrCreate()
		{
			if (connector != null)
				return connector;

			connector = new OctopusConnector();
			return connector;
		}

	}
}
