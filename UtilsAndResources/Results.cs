namespace UtilsAndResources
{
	public class Results
	{
		public Results()
		{
		}
		public Results(bool success, string[] errors)
		{
			Errors = errors;
			Succeded = success;
		}

		public string[] Errors { get; set; }
		public bool Succeded { get; set; }
	}
}