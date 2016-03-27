using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveMusicStreaming.DataModel
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class MusicDataItem
    {
        public MusicDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content)
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
    public class MusicDataGroup
    {
        public MusicDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<MusicDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<MusicDataItem> Items { get; private set; }
   
        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// MusicDataSource initializes with data read from a static json file from the 
    /// server.  This provides data at both design-time and run-time.
    /// </summary>
    public sealed class MusicDataSource
    {
        private static MusicDataSource _musicDataSource = new MusicDataSource();
        private IOneDriveClient ODClient;
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
                await _musicDataSource.GetMusicDataAsync();
                return _musicDataSource.Groups;
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
            await _musicDataSource.GetMusicDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _musicDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<MusicDataItem> GetItemAsync(string uniqueId)
        {
            await _musicDataSource.GetMusicDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _musicDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }


        private async Task GetMusicDataAsync()
        {

            string[] scopes = new string[] { "wl.signin", "wl.offline_access", "onedrive.readwrite" };
            string imgUrl = "";
            ODClient = OneDriveClientExtensions.GetUniversalClient(scopes, null, null);
            var auth = await ODClient.AuthenticateAsync();

            if (this._groups.Count != 0)
                return;
            try
            {
                var folders = await ODClient
                            .Drive
                            .Special["music"]
                            .Children
                            .Request()
                            .Expand("thumbnails")
                            .GetAsync();

                foreach (var i in folders)
                {
                    var items = await ODClient
                          .Drive
                          .Items[i.Id]
                          .Children
                          .Request()
                          .Expand("thumbnails")
                          .GetAsync();


                    foreach (var item in items)
                    {
                        var a = item.File.MimeType;
                    }
                    

                    try
                    {
                        if (items.Count > 0)
                            imgUrl = items[0].Thumbnails[0].Large.Url;
                        else
                            imgUrl = "";
                    }catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        imgUrl = "";
                    }

                    MusicDataGroup group = new MusicDataGroup(i.Id,
                                                            items.Count > 0 ? items.FirstOrDefault( a => a.File.MimeType == "audio/mpeg" || a.File.MimeType == "audio/mp4").Audio.Album : i.Name,
                                                            items.Count > 0 ? items.FirstOrDefault(a => a.File.MimeType == "audio/mpeg" || a.File.MimeType == "audio/mp4").Audio.Genre : "", 
                                                            imgUrl,
                                                            "");
                    
                    foreach (var item in items)
                    {
                        object urlDown = "";
                        IDictionary<string, object> testObj = item.AdditionalData as IDictionary<string, object>;
                        testObj.TryGetValue("@content.downloadUrl", out urlDown);

                        string[] extension = item.Name.Split('.');

                        try
                        {
                            if (extension.Contains("mp3") || extension.Contains("m4a"))
                            {
                                group.Items.Add(new MusicDataItem(item.Id,
                                                                  item.Audio.Title == "" ? "Audio" : item.Audio.Title,
                                                                  item.Audio.Genre,
                                                                  item.Thumbnails[0].Large.Url,
                                                                  extension.LastOrDefault<string>(),
                                                                  urlDown.ToString()));
                            }
                        }catch(ArgumentOutOfRangeException AOOREX)
                        {
                            Debug.WriteLine(AOOREX.Message);
                            if (extension.Contains("mp3") || extension.Contains("m4a"))
                            {
                                group.Items.Add(new MusicDataItem(item.Id,
                                                              item.Audio.Title == "" ? "Audio" : item.Audio.Title,
                                                              item.Audio.Genre,
                                                              "",
                                                              extension.LastOrDefault<string>(),
                                                              urlDown.ToString()));
                            }
                        }catch(Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                       
                    }

                    this.Groups.Add(group);
                }

            }
            catch (NullReferenceException nullrefex)
            {
                Debug.WriteLine("Please make sure your album doesn't have any additional folder files or non music files." + nullrefex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                await ODClient.SignOutAsync();
            }



        }
    }

}
