using OneDriveMusicStreaming.Common;
using OneDriveMusicStreaming.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDriveMusicStreaming.PlaylistViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistMainPage : Page
    {

        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }


        public PlaylistMainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            getData();
        }

        private async void getData()
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data

            var sampleDataGroups = await PlaylistDataSource.GetGroupsAsync();
           
            this.DefaultViewModel["Items"] = sampleDataGroups;
        }

        private void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var groupId = ((PlaylistDataGroup)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(PlaylistDetailsPage), groupId);
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.Frame != null && this.Frame.CanGoBack)
                this.Frame.GoBack();
        }
    }
}
