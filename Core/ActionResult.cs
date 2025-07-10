
namespace AetherFlow.Core
{
    public class ActionResult
    {
        public string Description { get; set; }
        public bool Success { get; set; }
        public List<string> LogEntries { get; set; }

        public ActionResult(string inDescription, bool inSuccess)
        {
            Description = inDescription;
            Success = inSuccess;

            LogEntries = new List<string>();
        }

    }//Action result class
}//namespace