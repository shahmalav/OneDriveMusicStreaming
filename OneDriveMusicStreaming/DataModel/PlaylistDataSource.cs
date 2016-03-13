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

    class PlaylistDataSource
    {
        private static PlaylistDataSource _playlistDataSource = new PlaylistDataSource();

        private ObservableCollection<MusicDataGroup> _groups = new ObservableCollection<MusicDataGroup>();
        public ObservableCollection<MusicDataGroup> Groups
        {
            get { return this._groups; }
            set { }
        }

        public static async Task<IEnumerable<MusicDataGroup>> GetGroupsAsync()
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

        public static async Task<MusicDataGroup> GetGroupAsync(string uniqueId)
        {
            await _playlistDataSource.GetPlaylistDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _playlistDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<MusicDataItem> GetItemAsync(string uniqueId)
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
                StorageFolder sFolder = ApplicationData.Current.LocalFolder;
                var file = await sFolder.GetFileAsync("Playlist.txt");
                jsonText = FileIO.ReadTextAsync(file).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
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
                MusicDataGroup group = new MusicDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["ImagePath"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new MusicDataItem(itemObject["UniqueId"].GetString(),
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
