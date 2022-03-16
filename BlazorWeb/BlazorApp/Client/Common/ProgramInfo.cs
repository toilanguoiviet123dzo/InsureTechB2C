namespace BlazorApp.Client.Common
{
    public class ProgramInfo
    {
        public ProgramInfo(string programID, string programName)
        {
            ProgramID = programID;
            ProgramName = programName;
        }
        public string ProgramID { get; set; } = "";
        public string ProgramName { get; set; } = "";
    }
}
