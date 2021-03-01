using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;


namespace COVID19_Statistics_Tracker
{
    /// <summary>
    /// This file contains code which runs as part of a viewmodel, providing and manipulating data to then be used on page.xaml.cs files
    /// This file contains class CoronaDataClass, which contains most of the methods pertaining to the collection of COVID-19 Data, and
    /// some struct classes, which are used to deserialise JSON data, and store important information easier.
    /// </summary>
    public class CoronaDataClass
    {
        //Setting up some data storage types.
        public Dictionary<string, string> CoronaCountryDict = new Dictionary<string, string>();
        public Dictionary<string, int> GraphData = new Dictionary<string, int>();
        //This will be called on initialisation of the class.
        public CoronaDataClass()
        {
            //generate the dictionary when the class is initialised. I think this was a really bad way of doing it in hindsight, and it should have been loaded on program
            //startup given its smaller size, but I think that in the end, this isn't really that impactful on performance, or readability, so it doesn't warrant me changing
            //it.
            CoronaCountryDict = CreateDictionary();
        }

        /// <summary>
        /// GetCoronaString() will be used to get COVID-19 statistics for that day, or yesterday should none be available, regarding
        /// many different bits of important information. It requires an ISO value as a parameter, to be able to find the correct
        /// information for specific countries.
        /// </summary>        
        /// <param name="CountryIso"></param>
        /// <returns name="stringList"></returns>        #can also return null
        public List<string> GetCoronaString(string CountryIso)
        {
            //creates a string to then write the json string to later, which contains the data that I wish to fetch.
            string json = "";
            //create a WebClient called Client
            using (WebClient client = new WebClient())
            {
                //create an exception catcher so the program doesnt crash if it hits an error downloading the data, caused by for example, the PC being offline, or the server
                //being down.
                try
                {
                    //Try to get all of the information for the statistics page from the current page for a specific country, using the country's
                    //ISO to fetch the correct page.
                    //Catch methods are below in case any of this goes wrong.
                    string casesDateString = "";
                    string deathsDateString = "";
                    string todayCases;
                    string todayDeaths;

                    //This line below will download the website's content as a string. As this page is a JSON file, this is a solid way to retrieve this data.
                    json = client.DownloadString($"https://disease.sh/v3/covid-19/countries/{CountryIso}");

                    //Here the JSON string is broken down into an object, and mapped to PrimaryCoronaData. This allows us to access individual parts of the JSON string.
                    var CoronaData2 = JsonConvert.DeserializeObject<PrimaryCoronaData>(json);
                    //This assigns the parts of the JSON string we need to string variables and then converts them to a nicer format. 
                    //API always uses a compatible format for this, so this has a low chance of causing an error. However, there is a catch clause
                    //for a FormatError below, just in case.
                    
                    string Cases = String.Format("{0:#,##0}", double.Parse(CoronaData2.cases));
                    string Active = String.Format("{0:#,##0}", double.Parse(CoronaData2.active));
                    string Deaths = String.Format("{0:#,##0}", double.Parse(CoronaData2.deaths));
                    string Caseper1mill = String.Format("{0:#,##0}", double.Parse(CoronaData2.casesPerOneMillion));
                    string Deathper1mill = String.Format("{0:#,##0}", double.Parse(CoronaData2.deathsPerOneMillion));
                    string TestsPerMill = String.Format("{0:#,##0}", double.Parse(CoronaData2.testsPerOneMillion));


                    //Logic that handles if the data for the country has not yet been updated for that specific day yet. If this is the case
                    //the program will check if there was any data for yesterday, and if there was, the program will use that
                    //If there is no data for yesterday as well, the program will assume that there are just no cases, and that
                    //the initial data was correct, so it shall go back to using it.
                    if (CoronaData2.todayCases != "0")
                    {
                        //note that this data was from today, and set the data to be the number of cases today, within the JSON object.
                        casesDateString = "Today";
                        todayCases = String.Format("{0:#,##0}", double.Parse(CoronaData2.todayCases));
                    }
                    //if the number of cases has not yet been updated in a country, get the number of cases from yesterday, this comes from issues like the UK statistics, which were always getting
                    //updated at irregular times in the day early on in the pandemic, which made it really hard to actually realise which data was from today, and which data was from yesterday.
                    else
                    {
                        //note that the data comes from yesterday, and not today.
                        casesDateString = "Yesterday";
                        //reset the JSON string to something else, with "?yesterday=true" being one of the parameters in the link which fetches the data, to get the data from yesterday, to attempt
                        //to offset this issue.
                        json = client.DownloadString($"https://disease.sh/v3/covid-19/countries/{CountryIso}?yesterday=true");
                        //make a new class object, and deserialise the new JSON object into it, and use the class SecondaryCoronaData, as the other objects required from CoronaData class, are not affected
                        //by this change in date, if the above issue is the case.
                        var CoronaData3 = JsonConvert.DeserializeObject<SecondaryCoronaData>(json);
                        todayCases = String.Format("{0:#,##0}", double.Parse(CoronaData3.todayCases));

                        if (todayCases == "0")
                        {
                            //note we are rewriting it with the data from today again, and do so.
                            casesDateString = "Today";
                            todayCases = String.Format("{0:#,##0}", double.Parse(CoronaData2.todayCases));
                        }
                    }

                    if (CoronaData2.todayDeaths != "0")
                    {
                        deathsDateString = "Today";
                        todayDeaths = String.Format("{0:#,##0}", double.Parse(CoronaData2.todayDeaths));
                    }

                    else
                    {
                        deathsDateString = "Yesterday";
                        json = client.DownloadString($"https://disease.sh/v3/covid-19/countries/{CountryIso}?yesterday=true");
                        var CoronaData3 = JsonConvert.DeserializeObject<SecondaryCoronaData>(json);
                        todayDeaths = String.Format("{0:#,##0}", double.Parse(CoronaData3.todayDeaths));

                        if (todayDeaths == "0")
                        {
                            deathsDateString = "Today";
                            todayDeaths = String.Format("{0:#,##0}", double.Parse(CoronaData2.todayDeaths));
                        }
                    }
                    
                    //If a country has not recorded this, just say "Data not found".
                    if (TestsPerMill == "0")
                    {
                        TestsPerMill = "Data not found.";
                    }
                    //Add all of the strings retrieved to a list to return to the class that requested it.
                    List<string> stringList = new List<string>(); 
                    stringList.Add(Cases);
                    stringList.Add(Deaths);
                    stringList.Add(Active);
                    stringList.Add(todayCases);
                    stringList.Add(todayDeaths);
                    stringList.Add(Caseper1mill);
                    stringList.Add(Deathper1mill);
                    stringList.Add(TestsPerMill);
                    return stringList;
                }

                catch (WebException)
                {
                    //If an error occurs in getting the data, do not return any of the data as it may be faulty. This will then be dealt with by the
                    //page asking for the data.
                    return null;
                }
                catch (FormatException)
                {
                    //If an error occurs in getting the data, do not return any of the data as it may be faulty. This will then be dealt with by the
                    //page asking for the data.
                    return null;
                }
            }
        }


        /// <summary>
        /// Uses country name passed on from naviagtion events in page classes, to find the ISO specific to that country, and returns
        /// it as a string variable.
        /// </summary>
        /// <param name="countryName"></param>
        /// <returns name="countryIso"></returns>
        public string GetCountryIso(string countryName)
        {
            //Try to get the ID from the country name.
            CoronaCountryDict.TryGetValue(countryName, out string countryIso);
            //Return the ID of the country
            return countryIso;
        }


        /// <summary>
        /// Gets data for graphs as part of viewmodel function of this class. Fetches historical data and deserialises it using
        /// struct GraphObjectData.
        /// </summary>
        /// <param name="countryISO"></param>
        /// <returns name="casesdict"></returns>            #Can also return null
        public async Task<Dictionary<string, int>> GetGraphCountryData(string countryISO)
        {
            try
            {
                HttpClient client = new HttpClient();
                string data = await client.GetStringAsync($"https://disease.sh/v3/covid-19/historical/{countryISO}?lastdays=all");
                JObject Jobj = JObject.Parse(data);
                var innerJObj = JObject.FromObject(Jobj["timeline"]);
                GraphObjectData dataclass = JsonConvert.DeserializeObject<GraphObjectData>(innerJObj.ToString());
                Dictionary<string, int> casesdict = dataclass.cases;
                return casesdict;
            }
            catch (WebException ex)
            {
                //Maybe in the future this could be the basis for a system that logs errors.
                return null;
            }
            catch (FormatException ex)
            {
                //Maybe in the future this could be the basis for a system that logs errors.
                return null;
            }
        }


        /// <summary>
        /// CreateDictionary() Creates a dictionary which is used to query ISO values specific to each country in the world.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> CreateDictionary()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("Afghanistan", "AF");
            dict.Add("Albania", "AL");
            dict.Add("Algeria", "DZ");
            dict.Add("Andorra", "AD");
            dict.Add("Angola", "AO");
            dict.Add("Anguilla", "AI");
            dict.Add("Argentina", "AR");
            dict.Add("Armenia", "AM");
            dict.Add("Aruba", "AW");
            dict.Add("Australia", "AU");
            dict.Add("Austria", "AT");
            dict.Add("Azerbaijan", "AZ");
            dict.Add("Bahamas", "BS");
            dict.Add("Bahrain", "BH");
            dict.Add("Bangladesh", "BD");
            dict.Add("Barbados", "BB");
            dict.Add("Belarus", "BY");
            dict.Add("Belgium", "BE");
            dict.Add("Belize", "BZ");
            dict.Add("Benin", "BJ");
            dict.Add("Bermuda", "BM");
            dict.Add("Bhutan", "BT");
            dict.Add("Bolivia", "BO");
            dict.Add("Bosnia", "BA");
            dict.Add("Botswana", "BW");
            dict.Add("Bouvet Island", "BV");
            dict.Add("Brazil", "BR");
            dict.Add("British Indian Ocean Territory", "IO");
            dict.Add("Brunei", "BN");
            dict.Add("Bulgaria", "BG");
            dict.Add("Burkina Faso", "BF");
            dict.Add("Burundi", "BI");
            dict.Add("Cabo Verde", "CV");
            dict.Add("Cambodia", "KH");
            dict.Add("Cameroon", "CM");
            dict.Add("Canada", "CA");
            dict.Add("Cayman Islands", "KY");
            dict.Add("Central African Republic", "CF");
            dict.Add("Caribbean Netherlands", "BQ");
            dict.Add("Chad", "TD");
            dict.Add("Chile", "CL");
            dict.Add("China", "CN");
            dict.Add("Christmas Island", "CX");
            dict.Add("Cocos (Keeling) Islands", "CC");
            dict.Add("Colombia", "CO");
            dict.Add("Comoros", "KM");
            dict.Add("Congo", "CG");
            dict.Add("DRC", "CD");
            dict.Add("Cook Islands", "CK");
            dict.Add("Costa Rica", "CR");
            dict.Add("Côte d'Ivoire", "CI");
            dict.Add("Croatia", "HR");
            dict.Add("Curaçao", "CW");
            dict.Add("Cyprus", "CY");
            dict.Add("Cuba", "CU");
            dict.Add("Czechia", "CZ");
            dict.Add("Denmark", "DK");
            dict.Add("Djibouti", "DJ");
            dict.Add("Dominica", "DM");
            dict.Add("Dominican Republic", "DO");
            dict.Add("Ecuador", "EC");
            dict.Add("Egypt", "EG");
            dict.Add("El Salvador", "SV");
            dict.Add("Equatorial Guinea", "GQ");
            dict.Add("Eritrea", "ER");
            dict.Add("Estonia", "EE");
            dict.Add("Ethiopia", "ET");
            dict.Add("Falkland Islands (Malvinas)", "FK");
            dict.Add("Faroe Islands", "FO");
            dict.Add("Fiji", "FI");
            dict.Add("Finland", "FJ");
            dict.Add("France", "FR");
            dict.Add("French Guiana", "GF");
            dict.Add("French Polynesia", "PF");
            dict.Add("French Southern Territories", "TF");
            dict.Add("Gabon", "GA");
            dict.Add("Gambia", "GM");
            dict.Add("Georgia", "GE");
            dict.Add("Germany", "DE");
            dict.Add("Ghana", "GH");
            dict.Add("Gibraltar", "GI");
            dict.Add("Greece", "GR");
            dict.Add("Greenland", "GL");
            dict.Add("Grenada", "GP");
            dict.Add("Guadeloupe", "GD");
            dict.Add("Guam", "GU");
            dict.Add("Guatemala", "GT");
            dict.Add("Guernsey", "GG");
            dict.Add("Guinea", "GN");
            dict.Add("Guyana", "GW");
            dict.Add("Guinea-Bissau", "GY");
            dict.Add("Haiti", "HT");
            dict.Add("Heard Island and McDonald Islands", "HM");
            dict.Add("Holy See (Vatican City State)", "VA");
            dict.Add("Honduras", "HN");
            dict.Add("Hong Kong", "HK");
            dict.Add("Hungary", "HU");
            dict.Add("Iceland", "IS");
            dict.Add("India", "IN");
            dict.Add("Indonesia", "ID");
            dict.Add("Iran", "IR");
            dict.Add("Iraq", "IQ");
            dict.Add("Ireland", "IE");
            dict.Add("Isle of Man", "IM");
            dict.Add("Israel", "IL");
            dict.Add("Italy", "IT");
            dict.Add("Jamaica", "JM");
            dict.Add("Japan", "JP");
            dict.Add("Channel Islands", "JE");
            dict.Add("Jordan", "JO");
            dict.Add("Kazakhstan", "KZ");
            dict.Add("Kenya", "KE");
            dict.Add("Kiribati", "KI");
            dict.Add("Kosovo", "XK");
            dict.Add("N. Korea", "KP");
            dict.Add("S. Korea", "KR");
            dict.Add("Kuwait", "KW");
            dict.Add("Kyrgyzstan", "KG");
            dict.Add("Lao People's Democratic Republic", "LA");
            dict.Add("Latvia", "LV");
            dict.Add("Lebanon", "LB");
            dict.Add("Lesotho", "LS");
            dict.Add("Liberia", "LR");
            dict.Add("Libyan Arab Jamahiriya", "LY");
            dict.Add("Liechtenstein", "LI");
            dict.Add("Lithuania", "LT");
            dict.Add("Luxembourg", "LU");
            dict.Add("Macao", "MO");
            dict.Add("Macedonia", "MK");
            dict.Add("Madagascar", "MG");
            dict.Add("Malawi", "MW");
            dict.Add("Malaysia", "MY");
            dict.Add("Maldives", "MV");
            dict.Add("Mali", "ML");
            dict.Add("Malta", "MT");
            dict.Add("Marshall Islands", "MH");
            dict.Add("Martinique", "MQ");
            dict.Add("Mauritania", "MR");
            dict.Add("Mauritius", "MU");
            dict.Add("Mayotte", "YT");
            dict.Add("Mexico", "MX");
            dict.Add("Micronesia", "FM");
            dict.Add("Moldova", "MD");
            dict.Add("Monaco", "MC");
            dict.Add("Mongolia", "MN");
            dict.Add("Montenegro", "ME");
            dict.Add("Montserrat", "MS");
            dict.Add("Morocco", "MA");
            dict.Add("Mozambique", "MZ");
            dict.Add("Myanmar", "MM");
            dict.Add("Burma", "BU");
            dict.Add("Namibia", "NA");
            dict.Add("Nauru", "NR");
            dict.Add("Nepal", "NP");
            dict.Add("Netherlands", "NL");
            dict.Add("Netherlands Antilles", "AN");
            dict.Add("New Caledonia", "NC");
            dict.Add("New Zealand", "NZ");
            dict.Add("Nicaragua", "NI");
            dict.Add("Niger", "NE");
            dict.Add("Nigeria", "NG");
            dict.Add("Niue", "NU");
            dict.Add("Norfolk Island", "NF");
            dict.Add("Northern Mariana Islands", "MP");
            dict.Add("Norway", "NO");
            dict.Add("Oman", "OM");
            dict.Add("Pakistan", "PK");
            dict.Add("Palau", "PW");
            dict.Add("Palestine", "PS");
            dict.Add("Panama", "PA");
            dict.Add("Papua New Guinea", "PG");
            dict.Add("Paraguay", "PY");
            dict.Add("Peru", "PE");
            dict.Add("Philippines", "PH");
            dict.Add("Pitcairn", "PN");
            dict.Add("Poland", "PL");
            dict.Add("Portugal", "PT");
            dict.Add("Puerto Rico", "PR");
            dict.Add("Qatar", "QA");
            dict.Add("Réunion", "RE");
            dict.Add("Romania", "RO");
            dict.Add("Russia", "RU");
            dict.Add("Rwanda", "RW");
            dict.Add("St. Barth", "BL");
            dict.Add("Saint Helena", "SH");
            dict.Add("Saint Kitts and Nevis", "KN");
            dict.Add("Saint Lucia", "LC");
            dict.Add("Saint Pierre Miquelon", "PM");
            dict.Add("Saint Martin", "MF");
            dict.Add("Sint Maarten", "SX");
            dict.Add("Saint Vincent and the Grenadines", "VC");
            dict.Add("Samoa", "WS");
            dict.Add("San Marino", "SM");
            dict.Add("Sao Tome and Principe", "ST");
            dict.Add("Saudi Arabia", "SA");
            dict.Add("Senegal", "SN");
            dict.Add("Serbia", "RS");
            dict.Add("Seychelles", "SC");
            dict.Add("Sierra Leone", "SL");
            dict.Add("Singapore", "SG");
            dict.Add("Slovakia", "SK");
            dict.Add("Slovenia", "SI");
            dict.Add("Solomon Islands", "SB");
            dict.Add("Somalia", "SO");
            dict.Add("South Africa", "ZA");
            dict.Add("South Georgia and the South Sandwich Islands", "GS");
            dict.Add("South Sudan", "SS");
            dict.Add("Spain", "ES");
            dict.Add("Sri Lanka", "LK");
            dict.Add("Sudan", "SD");
            dict.Add("Suriname", "SR");
            dict.Add("Svalbard and Jan Mayen", "SJ");
            dict.Add("Swaziland", "SZ");
            dict.Add("Sweden", "SE");
            dict.Add("Switzerland", "CH");
            dict.Add("Syrian Arab Republic", "SY");
            dict.Add("Taiwan", "TW");
            dict.Add("Tajikistan", "TJ");
            dict.Add("Tanzania", "TZ");
            dict.Add("Thailand", "TH");
            dict.Add("Timor-Leste", "TL");
            dict.Add("Togo", "TG");
            dict.Add("Tokelau", "TK");
            dict.Add("Tonga", "TO");
            dict.Add("Trinidad and Tobago", "TT");
            dict.Add("Tunisia", "TN");
            dict.Add("Turkey", "TR");
            dict.Add("Turkmenistan", "TM");
            dict.Add("Turks and Caicos Islands", "TC");
            dict.Add("Tuvalu", "TV");
            dict.Add("Uganda", "UG");
            dict.Add("Ukraine", "UA");
            dict.Add("UAE", "AE");
            dict.Add("UK", "GB");
            dict.Add("USA", "US");
            dict.Add("United States Minor Outlying Islands", "UM");
            dict.Add("Uruguay", "UY");
            dict.Add("Uzbekistan", "UZ");
            dict.Add("Vanuatu", "VU");
            dict.Add("Venezuela", "VE");
            dict.Add("Vietnam", "VN");
            dict.Add("British Virgin Islands", "VG");
            dict.Add("U.S. Virgin Islands", "VI");
            dict.Add("Wallis and Futuna", "WF");
            dict.Add("Western Sahara", "EH");
            dict.Add("Yemen", "YE");
            dict.Add("Zambia", "ZM");
            dict.Add("Zimbabwe", "ZW");
            return dict;
        }     
    }


    /// <summary>
    /// Struct for mapping today's COVID-Statistics to.
    /// </summary>
    internal struct PrimaryCoronaData
    {
        public string cases { get; set; }
        public string deaths { get; set; }
        public string todayCases { get; set; }
        public string todayDeaths { get; set; }
        public string country { get; set; }
        public string active { get; set; }
        public string casesPerOneMillion { get; set; }
        public string deathsPerOneMillion { get; set; }
        public string testsPerOneMillion { get; set; }
    }


    /// <summary>
    /// Struct for mapping yesterday's cases and deaths statistics to
    /// </summary>
    internal struct SecondaryCoronaData
    {
        public string todayCases { get; set; }
        public string todayDeaths { get; set; }
    }


    /// <summary>
    /// Struct for mapping historical COVID-19 Data to.
    /// </summary>
    internal struct GraphObjectData
    {
        public Dictionary<string, int> cases { get; set; }
        //other two could be used in future to make multiple graphs.
        public Dictionary<string, int> deaths { get; set; }
        public Dictionary<string, int> recovered { get; set; }        
    } 
}
