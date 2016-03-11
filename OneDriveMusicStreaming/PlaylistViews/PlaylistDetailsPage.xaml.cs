﻿using OneDriveMusicStreaming.Common;
using OneDriveMusicStreaming.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
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
    public sealed partial class PlaylistDetailsPage : Page
    {

        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private SystemMediaTransportControls systemMediaControls;
        private DispatcherTimer _timer;


        public PlaylistDetailsPage()
        {
            this.InitializeComponent();
            systemMediaControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            systemMediaControls.ButtonPressed += SystemControls_ButtonPressed;

            // Register to handle the following system transpot control buttons.
            systemMediaControls.IsPlayEnabled = true;
            systemMediaControls.IsPauseEnabled = true;
        }

        private async void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        MusicPlayer.Play();
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        MusicPlayer.Pause();
                    });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        PlaylistDataGroup group;


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            group = await PlaylistDataSource.GetGroupAsync((String)e.Parameter);
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items;

        }

        private void StartTimer()
        {
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= TimerTick;
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            StartTimer();
        }

        private void TimerTick(object sender, object e)
        {
            audioTimeline.Text = MusicPlayer.Position.ToString(@"mm\:ss");
        }

        private void MusicPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            SetupTimer();

            if (MusicPlayer.Source != null)
            {
                // Get the updater.
                // SystemMediaTransportControlsDisplayUpdater updater = systemMediaControls.DisplayUpdater;

                //await updater.CopyFromFileAsync(MediaPlaybackType.Music, MusicPlayer.Source.);

                // Update the system media transport controls
                //updater.Update();
            }
        }

        private void MusicPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (MusicPlayer.CurrentState)
            {
                case MediaElementState.Playing:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    PlayButton.Icon = new SymbolIcon(Symbol.Pause);
                    break;
                case MediaElementState.Paused:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    PlayButton.Icon = new SymbolIcon(Symbol.Play);
                    break;
                case MediaElementState.Stopped:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    PlayButton.Icon = new SymbolIcon(Symbol.Play);
                    break;
                case MediaElementState.Closed:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    PlayButton.Icon = new SymbolIcon(Symbol.Play);
                    break;
                default:
                    break;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (MusicPlayer.CurrentState == MediaElementState.Playing)
            {
                MusicPlayer.Pause();
                PlayButton.Icon = new SymbolIcon(Symbol.Pause);
            }
            else
            {
                MusicPlayer.Play();
                PlayButton.Icon = new SymbolIcon(Symbol.Play);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemListView.SelectedIndex == itemListView.Items.Count - 1)
                itemListView.SelectedIndex = 0;
            else
                itemListView.SelectedIndex = ++itemListView.SelectedIndex;

        }

        private void FavButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemListView.SelectedIndex == 0)
                itemListView.SelectedIndex = itemListView.Items.Count - 1;
            else
                itemListView.SelectedIndex = --itemListView.SelectedIndex;

        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.Frame != null && this.Frame.CanGoBack)
                this.Frame.GoBack();
        }
    }
}
