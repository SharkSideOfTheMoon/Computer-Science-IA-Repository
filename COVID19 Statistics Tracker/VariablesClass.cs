namespace COVID19_Statistics_Tracker
{
    /// <summary>
    /// This class is used as a format for the "parameter" navigation parameter, which relays important information in the form of an object during
    /// a navigation event. This is then used to set up pages correctly.
    /// </summary>
    public class VariablesClass
    {
        public string CountryName { get; set; }
        //This variable is used to tell the program what navigationviewer was used in the event... was it the main one (side bar) or not (top bar).
        public bool MainNavEvent { get; set; }
    }
}
