using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace COVID19_Statistics_Tracker
{
    /// <summary>
    /// This is a class file assigned to page type CountryPage. This page is used to display statistic data for countries. It is the default page
    /// type that is navigated to when someone clicks the button of a country in the navigation bar on the left of the screen.
    /// </summary>
    public sealed partial class CountryPage : Page
    {
        //Set up string variable for country name to be assigned to
        private string countryName;

        public CountryPage()
        {
            this.InitializeComponent();
            
            //GUI setup:
            NavView.IsPaneVisible = true;
            NavView.IsHitTestVisible = true;
            
            //Set event triggers to call methods below.
            NavView.PaneOpening += ShowBar;
            NavView.PaneClosing += HideBar;
            

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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Parameters let the page know what country the user is requesting information for.
            var parameters = (VariablesClass)e.Parameter;
            this.countryName = parameters.CountryName;

            if (countryName != null)
            {
                await StartUp(countryName);
            }
        }
        

        /// <summary>
        /// StartUp() sets up page objects on the navigation to the page, and begins to fetch all of the information to then display, once page is
        /// fully loaded.
        /// </summary>
        /// <param name="PageName"></param>
        /// <returns></returns>
        private async Task StartUp(string PageName)
        {
            CoronaDataClass c = new CoronaDataClass();
            Dictionary<string, string> CountryDict = c.CoronaCountryDict;
            string countryname = PageName;
            
            //Gets the country's ID from the dictionary, handled within the class the dictionary is stored in, as this is good practice.
            string countryIso = c.GetCountryIso(countryname);

            //Then create a list of information based on parts of the JSON data that the API would return for that country.
            List<string> infoList = new List<string>();
            //Runs asynchronously so it doesn't take up the main computational thread while this is happening. 
            await Task.Run(() =>
            {
                infoList = c.GetCoronaString(countryIso);
            });
            
            //If this has actually returned something, create a string to then display as a message on the statistics page.
            if(infoList != null)
            {
                string text = String.Format($"{countryname}:\n\n" +
                    $"Cases: {infoList[0]}\n" +
                    $"Deaths: {infoList[1]}\n" +
                    $"Active Cases: {infoList[2]}\n" +
                    $"Cases Found Today: {infoList[3]}\n" +
                    $"Deaths Today: {infoList[4]}\n" +
                    $"Cases Per 1 Million Population: {infoList[5]}\n" +
                    $"Deaths Per 1 Million Population: {infoList[6]}\n" +
                    $"Tests Per 1 Million Population: {infoList[7]}\n");
                
                //Write the text to the textbox object on the page, using special dispatcher that must be used to do so, as this is always done
                //on the GUI thread, and not on the main thread. GUI thread must be used for all GUI based changes.
                await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //Before this is done, text will read "information loading..."
                    //This will replace it with the string which was formatted before.
                    Tester.Text = text; 
                });
            }
            else
            {
                //If no data was found, or an error occurred, the list will be returned as null. This is then handled by changing the text on the
                //page through the GUI thread, to say "Data incomplete or not found" "Please return to home page."
                await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Tester.Text = "Data Incomplete or Not Found.\n\nPlease return to the Home Page.";
                    TopNavBar.IsHitTestVisible = false;
                });
            }            
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
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                var PageContentTag = args.InvokedItemContainer.Content.ToString();
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
        /// Navigation method for main navigation bar. Navigates to other pages using information gathered before-hand.
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
            //Same navigation idea, but this time for the top bar.
            Type _page = null;
                        
            var item = _pagesTop.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            _page = item.Page;
            
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = this.Frame.CurrentSourcePageType;

            var parameters = new VariablesClass();
            parameters.CountryName = countryName;

            // Only navigate if the selected page isn't currently loaded.
            parameters.MainNavEvent = false;
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
    }
}