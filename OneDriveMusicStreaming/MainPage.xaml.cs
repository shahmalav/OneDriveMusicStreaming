using OneDriveMusicStreaming.Common;
using OneDriveMusicStreaming.DataModel;
using OneDriveMusicStreaming.PlaylistViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OneDriveMusicStreaming
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        private ObservableDictionary defaultViewModel = new ObservableDictionary();


        public MainPage()
        {
          
            this.InitializeComponent();

        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!this.Frame.CanGoBack)
            {
                backButton.Visibility = Visibility.Collapsed;
            }
            getData();
        }

       
        private async void getData()
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = await MusicDataSource.GetGroupsAsync();
            this.DefaultViewModel["Items"] = sampleDataGroups;

        }

        private void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var groupId = ((MusicDataGroup)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(DetailMusicPage), groupId);
        }

        private void btnAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PlaylistMainPage));
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
