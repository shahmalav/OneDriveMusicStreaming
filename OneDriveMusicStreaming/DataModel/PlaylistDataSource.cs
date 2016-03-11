using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace OneDriveMusicStreaming.DataModel
{

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class PlaylistDataItem
    {
        public PlaylistDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class PlaylistDataGroup
    {
        public PlaylistDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<PlaylistDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<PlaylistDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }



    class PlaylistDataSource
    {
        private static PlaylistDataSource _playlistDataSource = new PlaylistDataSource();

        private ObservableCollection<PlaylistDataGroup> _groups = new ObservableCollection<PlaylistDataGroup>();
        public ObservableCollection<PlaylistDataGroup> Groups
        {
            get { return this._groups; }
            set { }
        }

        public static async Task<IEnumerable<PlaylistDataGroup>> GetGroupsAsync()
        {

            try
            {
                await _playlistDataSource.GetPlaylistDataAsync();
                return _playlistDataSource.Groups;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.InnerException.Message);
                //ex.Message;
                return null;
            }
        }

        public static async Task<PlaylistDataGroup> GetGroupAsync(string uniqueId)
        {
            await _playlistDataSource.GetPlaylistDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _playlistDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<PlaylistDataItem> GetItemAsync(string uniqueId)
        {
            await _playlistDataSource.GetPlaylistDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _playlistDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        
        private async Task GetPlaylistDataAsync()
        {
            if (this._groups.Count != 0)
                return;
            string jsonText = "";
            try
            {
                string filename = "ms-appx:///DataModel/playlist.txt";

                Uri appUri = new Uri(filename);//File name should be prefixed with 'ms-appx:///Assets/* 

             //   StorageFile anjFile = StorageFile.GetFileFromApplicationUriAsync(appUri).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                StorageFile anjFile = await StorageFile.GetFileFromApplicationUriAsync(appUri);
                jsonText = FileIO.ReadTextAsync(anjFile).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                jsonText = "" + ex.Message;
            }


            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                PlaylistDataGroup group = new PlaylistDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["ImagePath"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new PlaylistDataItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["ImagePath"].GetString(),
                                                       itemObject["Description"].GetString(),
                                                       itemObject["Content"].GetString()));
                }
                this.Groups.Add(group);
            }
        }
    }
}
