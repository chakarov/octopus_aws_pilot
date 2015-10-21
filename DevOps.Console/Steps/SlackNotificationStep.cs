namespace DevOps.Console.Steps
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.Net;
	using System.Text;

	public class SlackNotificationStep : IDeployStep
	{
		private readonly InstanceDeploy instance;
		private readonly bool deployStatus;
		private readonly string error;
		public string Name { get; set; }
		public string Error { get; set; }
		public bool FinishedSuccessfully { get; set; }

		private readonly Encoding _encoding = new UTF8Encoding();  

		public SlackNotificationStep(InstanceDeploy instance, bool deployStatus, string error)
		{
			this.instance = instance;
			this.deployStatus = deployStatus;
			this.error = error;

			Name = "Sending notification to Slack";
		}

		public void Run()
		{
			try
			{
				var uri = new Uri(ConfigurationManager.AppSettings["slack-uri"]);

				var logLink = string.Format("<Link-here|Click here>", instance.Branch, instance.Release, instance.DeploymentId);

				var successMsg = string.Format("*Automated Deploy Finish with Success!*\n Branch {0}\n Release {1}\n Logs: {2}", instance.Branch, instance.Release, logLink);

				var errorMsg = string.Format("*Automated Deploy Finish with Error!*\n Branch: {0}\n Release {1}\n Logs: {2}\nError: {3}", instance.Branch, instance.Release, logLink, error);

				var payload = new Payload()
				{
					Channel = "",
					IconUrl = "http://octopusdeploy.com/content/resources/favicon.png",
					Username = "",
					Text = (deployStatus) ? successMsg : errorMsg
				};

				var payloadJson = JsonConvert.SerializeObject(payload);  
  
				using (var client = new WebClient())  
				{  
					var data = new NameValueCollection();  
					data["payload"] = payloadJson;  
  
					var response = client.UploadValues(uri, "POST", data);  
  
					var responseText = _encoding.GetString(response);  
				}  
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}
		}
	}

	public class Payload  
	{  
		[JsonProperty("channel")]  
		public string Channel { get; set; }  
  
		[JsonProperty("username")]  
		public string Username { get; set; }  
  
		[JsonProperty("text")]  
		public string Text { get; set; }

		[JsonProperty("icon_url")]
		public string IconUrl { get; set; }
	}
}  
  

