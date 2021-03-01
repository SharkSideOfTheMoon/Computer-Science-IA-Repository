using System;
using System.Globalization;
using System.Collections.ObjectModel;

namespace COVID19_Statistics_Tracker
{
    /// <summary>
    /// This class is used to create an ObervableCollection collection which can then be used to bind to the chart on a CountryGraphPage. This
    /// class reformts the dates into usable DateTime Objects, and maps them to a DayData class, where they are then added as parts to an observable
    /// collection.
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// List to bind to chart data.
        /// </summary>
        public ObservableCollection<DayData> Data { get; set; }
        
        /// <summary>
        /// Reformats dates and creates cllection that is then used. called on initialisation of the class.
        /// </summary>
        /// <param name="Dates"></param>
        /// <param name="CaseNumbers"></param>
        public ViewModel(string[] Dates, int[] CaseNumbers)
        {            
            //Create new instance of the collection
            Data = new ObservableCollection<DayData>();

            //format the dates into usable DateTime object formats.
            for (int i = 0; i < Dates.Length; i++)
            {
                string[] spltTime = Dates[i].Split('/');
                if(spltTime[0].Length == 1)
                {
                    spltTime[0] = $"0{spltTime[0]}";
                }
                if (spltTime[1].Length == 1)
                {
                    spltTime[1] = $"0{spltTime[1]}";
                }
                if (spltTime[2].Length == 1)
                {
                    spltTime[2] = $"0{spltTime[2]}";
                }
                Dates[i] = $"{spltTime[1]}/{spltTime[0]}/20{spltTime[2]}";
                
                //Add the data to new DayData objects, and then add these objects to the observable collection to be used.
                Data.Add(new DayData { DateTimeVar = (DateTime.ParseExact(Dates[i],"dd'/'M'/'yyyy" ,CultureInfo.InvariantCulture)), CaseNumber = CaseNumbers[i] });
            }            
        }
    }


    /// <summary>
    /// Class used to create a map of historical data.
    /// </summary>
    public class DayData
    {
        public DateTime DateTimeVar { get; set; }
        public int CaseNumber { get; set; }
    }
}
