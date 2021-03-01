using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace COVID19_Statistics_Tracker
{
    /// <summary>
    /// This is the page that is navigated to when a user has visited CountryPage, with a specified country, and then has navigated
    /// to "Graphs" in the navigation bar at the top of their screen. This will then load this page, and notify this page of what
    /// country it should be fetching data for.
    /// </summary>
    public sealed partial class CountryGraphPage : Page
    {
        //Set up string variable for country name to be assigned to
        private string CountryName;

        /// <summary>
        /// Called on intialisation of this class. 
        /// </summary>
        public CountryGraphPage()
        {
        
            this.InitializeComponent();

            //Navview and SViewer setup, found this worked best as a code function, rather than setting this in XAML. There were a few inconsistencies
            //when XAML was used to set these variables, and so I moved this to the top of each page.xaml.cs file.
            NavView.IsPaneVisible = true;
            NavView.IsHitTestVisible = true;
            SViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            SViewer.VerticalScrollMode = ScrollMode.Disabled;
            //Events set so program knows what to do when these events are called. Just in case navview closes by accident for some reason, provides user with a way to
            //expand it again. Might be useful in future if want to provide option for a non fixed navigation bar.
            NavView.PaneOpening += ShowBar;
            NavView.PaneClosing += HideBar;

            //Corresponding methods. Void used here as void is preferrable over tasks or other return types for usage in event handling.
            void ShowBar(NavigationView NavView, object sender)
            {
                SViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                NavView.IsPaneVisible = true;
                SViewer.VerticalScrollMode = ScrollMode.Enabled;
            }
            void HideBar(object sender, NavigationViewPaneClosingEventArgs e)
            {
                SViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                SViewer.VerticalScrollMode = ScrollMode.Disabled;
            }
        }


        //Set up the information for the page before page is loaded. Perform this once page is navigated to.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Parameters let the page know what country the user is requesting information for.
            var parameters = (VariablesClass)e.Parameter;
            CountryName = parameters.CountryName;
        }


        /// <summary>
        /// List of page types, and corresponding tag.
        /// </summary>
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("home", typeof(MainPage)),
            ("Country", typeof(CountryPage)),
        };

        //See above, but this time for the navigation bar at the top of the screen.
        private readonly List<(string Tag, Type Page)> _pagesTop = new List<(string Tag, Type Page)>
        {
            ("Statistics", typeof(CountryPage)),
            ("Graphs", typeof(CountryGraphPage)),
        };


        /// <summary>
        /// Page setup after navigation to this page / loading of this page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Add handler for this.Frame navigation.
            this.Frame.Navigated += (object sender2, NavigationEventArgs eventArgs) =>
            {
                //Create paramaeters based on the parameters passed on as the Parameter object in NavigationEventsArgs. This will always be of type
                //VariablesClass, as this is what was set in all of my NavView_Navigate() methods.
                VariablesClass parameters = (VariablesClass)eventArgs.Parameter;

                if (parameters.MainNavEvent == true)
                {
                    //Select first item in the menu to reset position once navigation to this page occurs.
                    On_Navigated(sender2, eventArgs);
                    NavView.SelectedItem = NavView.MenuItems[0];
                }
            };
        }


        /// <summary>
        /// Page setup after navigation to this page / loading of this page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavTopBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Add handler for this.Frame navigation
            this.Frame.Navigated += (object sender2, NavigationEventArgs eventArgs) =>
            {
                //Create paramaeters based on the parameters passed on as the Parameter object in NavigationEventsArgs. This will always be of type
                //VariablesClass, as this is what was set in all of my TopNavBar_Navigate() methods.
                VariablesClass parameters = (VariablesClass)eventArgs.Parameter;

                //If the navigation event was related to the top bar, change the selection
                if (parameters.MainNavEvent == false)
                {
                    //Select first item in the menu to reset position once navigation to this page occurs.
                    On_Navigated_TopBar(sender2, eventArgs);
                    TopNavBar.SelectedItem = TopNavBar.MenuItems[0];
                }
            };
        }


        /// <summary>
        /// Event handler and call for when a button is pressed on the navigation menu. This will then run the navigation method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavView_ItemInvoked(NavigationView sender,
                                         NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer != null)
            {
                //Check that the a navigation view item has actually been invoked, and if it has retrieve the tag assigned in XAML markup to that
                //specific button.
                //Then get the content written onto the button, and use it to find out which country / mode (eg: stats or graphs) the User is trying
                //to navigate to.
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                var PageContentTag = args.InvokedItemContainer.Content.ToString();

                //Then use this information gathered above, to attempt to navigate to a page using this information, and the reccomended naviagation
                //transition information. (eg: animations, etc...)
                NavView_Navigate(navItemTag, PageContentTag, args.RecommendedNavigationTransitionInfo);
            }
        }


        /// <summary>
        /// Event handler and call for when a button is pressed on the navigation menu. This will then run the navigation method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TopNavBar_ItemInvoked(NavigationView sender,
                                         NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer != null)
            {
                //Check that the a navigation view item has actually been invoked, and if it has retrieve the tag assigned in XAML markup to that
                //specific button.
                //Then get the content written onto the button, and use it to find out which country / mode (eg: stats or graphs) the User is trying
                //to navigate to.
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                var PageContentTag = args.InvokedItemContainer.Content.ToString();

                //Then use this information gathered above, to attempt to navigate to a page using this information, and the reccomended naviagation
                //transition information. (eg: animations, etc...)
                TopNavBar_Navigate(navItemTag, PageContentTag, args.RecommendedNavigationTransitionInfo);                
            }
        }


        /// <summary>
        /// Navigation method. Navigates to other pages using information gathered before-hand.
        /// </summary>
        /// <param name="navItemTag"></param>
        /// <param name="navItemContent"></param>
        /// <param name="transitionInfo"></param>
        private void NavView_Navigate(
            string navItemTag,
            string navItemContent,
            Windows.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            _page = item.Page;

            // Get the page type before navigation so you can prevent duplicate
            // entries.
            var preNavPageType = this.Frame.CurrentSourcePageType;

            var parameters = new VariablesClass();
            parameters.CountryName = navItemContent;

            // Only navigate if the selected page isn't currently loaded.
            parameters.MainNavEvent = true;

            //This code is to avoid the threads staying in use after navigating away and freezing the program due to a library issue. This fixes that.
            dChart.Dispatcher.StopProcessEvents();
            dChart.Dispose();

            this.Frame.Navigate(_page, parameters, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
        }


        /// <summary>
        /// Navigation method for secondary navigation bar. Navigates to page with other types of COVID information.
        /// </summary>
        /// <param name="navItemTag"></param>
        /// <param name="navItemContent"></param>
        /// <param name="transitionInfo"></param>
        private void TopNavBar_Navigate(
            string navItemTag,
            string navItemContent,
            Windows.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
        {
            //same idea but for the top navigation bar
            Type _page = null;

            var item = _pagesTop.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            _page = item.Page;

            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = this.Frame.CurrentSourcePageType;

            var parameters = new VariablesClass();
            parameters.CountryName = CountryName;

            // Only navigate if the selected page isn't currently loaded.
            parameters.MainNavEvent = false;
            dChart.Dispatcher.StopProcessEvents();
            dChart.Dispose();
            //This code above is to avoid the threads staying in use after navigating away and freezing the program due to a library issue. This fixes that.
            this.Frame.Navigate(_page, parameters, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
        }


        /// <summary>
        /// Part of Page setup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            //Part of page setup, for selecting first item in NavBar when page navigated to
            if (this.Frame.SourcePageType != null && this.Frame.SourcePageType != typeof(CountryGraphPage))
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));

                NavView.Header =
                    ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
            }
        }


        /// <summary>
        /// Part of Page setup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_Navigated_TopBar(object sender, NavigationEventArgs e)
        {
            //Part of page setup, for selecting first item in NavBar when page navigated to
            if (this.Frame.SourcePageType != null)
            {
                var item = _pagesTop.FirstOrDefault(p => p.Page == e.SourcePageType);

                TopNavBar.SelectedItem = TopNavBar.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));

                TopNavBar.Header =
                    ((NavigationViewItem)TopNavBar.SelectedItem)?.Content?.ToString();
            }
        }


        /// <summary>
        /// Runs when the chart object on the page loads. Sets up the chart with the correct information, and format.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void chart_Loaded(object sender, RoutedEventArgs e)
        {
            //Initialise an instance of CoronaDataClass to deal with data handling in this section
            CoronaDataClass c = new CoronaDataClass();
            Dictionary<string, int> dict;

            //get the ISO value to query the API with per country.
            string countryIso = c.GetCountryIso(CountryName);
            
            //Start formatting and creating the chart
            string[] datearray;
            int[] casesarray;
            //Creation of the graph header.
            dChart.Header = $"{CountryName} Cases Graph.";
            //Setting dictionary to the data fetched by the function to fetch graph data within context in CoronaDataClass.cs
            dict = await c.GetGraphCountryData(countryIso);
            try
            {
                //Set arrays for dates and cases because the length of these lists will not change until page is refreshed anyway.
                datearray = dict.Keys.ToArray();
                casesarray = dict.Values.ToArray();
                //create a new line series chart using Syncfusion. Most of this code sets values and variables for the Chart on the page
                Syncfusion.UI.Xaml.Charts.LineSeries series = new Syncfusion.UI.Xaml.Charts.LineSeries();
                series.ItemsSource = (new ViewModel(datearray, casesarray)).Data;
                series.XBindingPath = "DateTimeVar";
                series.YBindingPath = "CaseNumber";
                series.Label = $"{CountryName}";
                series.Interior = new SolidColorBrush(Colors.Black);
                series.StrokeThickness = 2;
                
                //set the minimum and maximum values for the axes'
                CaseNumberAxis.Maximum = casesarray.Max();
                CaseNumberAxis.Minimum = casesarray.Min();
                

                //format the dates, could have used Regex, but performance difference is negligable and this was easier.
                DateTime[] dates = new DateTime[datearray.Length];

                for (int i = 0; i < datearray.Length; i++)
                {
                    string[] spltTime = datearray[i].Split('/');
                    if (spltTime[0].Length == 1)
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
                    datearray[i] = $"{spltTime[0]}/{spltTime[1]}/{spltTime[2]}";
                    dates.Append((DateTime.ParseExact(datearray[i], "dd'/'M'/'yyyy", CultureInfo.InvariantCulture)));
                    //create a new list of dates
                }
                DateTimeAxis.Minimum = DateTime.ParseExact(datearray[0], "dd'/'M'/'yyyy", CultureInfo.InvariantCulture);
                DateTimeAxis.Maximum = DateTime.ParseExact(datearray[datearray.Length - 1], "dd'/'M'/'yyyy", CultureInfo.InvariantCulture);
                //apply the dates to the chart
                dChart.Series.CollectionChanged += (object sender2, System.Collections.Specialized.NotifyCollectionChangedEventArgs e2) =>
                {
                    //anything you want to do when a the collection changes, would be put in here. This is just here in case anyone wants to
                    //do anything with this in the future. :)
                };
                //when the data is added, change the chart from the default chart
                dChart.Series.Add(series);

                //Use GUI dispatcher to instruct GUI thread to edit text on the page.
                await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //change "information loading" text
                    Tester.Text = $"Graph of number of cases in {CountryName}";
                });
            }
            //In case of no historical data on API for this country...
            catch (NullReferenceException ex)
            {
                //do nothing otherwise as no info.
                //for when no information is found, or information is incomplete due to API error                
                await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //change "information loading" text
                    Tester.Text = $"Could not find historical information for: {CountryName}";
                    //hide chart if no data
                    dChart.Visibility = Visibility.Collapsed;
                    White_rect_2.Visibility = Visibility.Collapsed;
                    
                });

            }
        }
    }
}
