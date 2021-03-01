﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace COVID19_Statistics_Tracker
{
    /// <summary>
    /// ---Project Summary & Notes---
    /// 
    /// ---C# files and compilation of project---
    /// 
    /// This is the main file of the program (with relation to code written by student). This page handles the scripting and logic behind
    /// the home page of the application. This page will therefore load on the opening of the application, as this is the first page
    /// navigated to.
    /// 
    /// The page contains a naviagion system includnig methods and lists that are uniform throughout the program. Therefore, I shall
    /// attepmt to not comment on these naviagtion methods outside of this class file, as to avoid a misconcpetion that these function
    /// differently.
    /// 
    /// This page, and all other .xaml.cs files in this program are script files *linked* to a XAML page. Therefore, you may see some
    /// things from the XAML file referenced in these files, without having been defined first. This is not an issue, as when compiled all
    /// together, this will not cause errors.
    /// 
    /// To run and compile this program, you will need to visit a github repository linked on the "Cover Page" document and download the
    /// entire project. This is because the program was too large to upload within the IA file size limit otherwise. The full project
    /// should take up around 2 GB on disk.
    /// 
    /// 
    /// ---XAML files---
    /// 
    /// There is also regrettably no way to comment in XAML files. Therefore, all page.xaml files will not have comments in them explaining them.
    /// However, should you load this project in Visual Studio Blend... you should be able to play aronud with the features, and understand why certain
    /// values are the way they are.
    /// 
    /// Around 2/3 of the code in the XAML files is autogenerated from GUI interaction with an IDE to create a GUI however. The 1/3 that is not this way
    /// is regrettably not able to be commented on. This includes features such as the trackball on the Syncfusion Graphs, which is a behaviour assigned
    /// in the XAML files to these graphs.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        /// <summary>
        /// Called on initialisation of this class.
        /// </summary>
        public MainPage()
        {
            //initilaise the component.
            this.InitializeComponent();
        }
        

        /// <summary>
        /// List of page types, and corresponding tag.
        /// </summary>
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("home", typeof(MainPage)),
            ("Country", typeof(CountryPage)),
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
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                parameters.MainNavEvent = true;
                this.Frame.Navigate(_page, parameters, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
            }
        }


        /// <summary>
        /// Part of Page setup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            //Part of page setup, for selecting first item in NavBar when page navigated to
            if (this.Frame.SourcePageType != null)
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));                
            }
        }


        /// <summary>
        /// Event handler and call for the pressing of the API button on the home page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnclickSource(object sender, RoutedEventArgs e)
        {
            //Does not need a try catch, as if no internet connection, it will still try to open this in the default browser, and that should tell you
            //that there is no internet connection, letting you know something is wrong.
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://disease.sh/"));
        }
    }
}
