namespace DevOps.Console
{
	public interface IDeployStep
	{
		string Name { get; set; }
		string Error { get; set; }
		bool FinishedSuccessfully { get; set; }

		void Run();
	}
}
