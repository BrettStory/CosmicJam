namespace CosmicJam.Library.Services {

    using Macabre2D.Framework;
    using Macabre2D.Wpf.Common.Services;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    public interface ISongService : INotifyPropertyChanged, IChangeDetectionService {
        Song CurrentSong { get; }

        Track CurrentTrack { get; set; }

        Task<Song> CreateSong();

        Task<Song> LoadSong();

        Task<bool> SaveSong();

        Task<bool> SaveSongAs();
    }

    public sealed class SongService : NotifyPropertyChanged, ISongService {
        public const string SongFileExtension = "cosmicjam";
        public const string SongFileFilter = "Cosmic Jam File (*." + SongFileExtension + ")|*." + SongFileExtension;
        private readonly ICommonDialogService _dialogService;
        private Song _currentSong = new Song();
        private string _currentSongPath;
        private Track _currentTrack;
        private bool _hasChanges;

        public SongService(ICommonDialogService dialogService) {
            this._dialogService = dialogService;
            this._currentSong = new Song();
            this._currentTrack = this._currentSong.Tracks.First();
        }

        public IReadOnlyCollection<Track> AvailableTracks {
            get {
                return this.CurrentSong.Tracks;
            }
        }

        public Song CurrentSong {
            get {
                return this._currentSong;
            }

            private set {
                if (value != null && this.Set(ref this._currentSong, value)) {
                    this.CurrentTrack = this.AvailableTracks.First();
                }
            }
        }

        public Track CurrentTrack {
            get {
                if (this._currentTrack == null) {
                    this._currentTrack = this.AvailableTracks.First();
                }

                return this._currentTrack;
            }

            set {
                if (value != null) {
                    this.Set(ref this._currentTrack, value);
                }
            }
        }

        public bool HasChanges {
            get {
                return this._hasChanges;
            }

            set {
                this.Set(ref this._hasChanges, value);
            }
        }

        public async Task<Song> CreateSong() {
            Song result = null;
            if (this.AskToSave() != MessageBoxResult.Cancel) {
                this._currentSongPath = null;
                this.CurrentSong = new Song();
                await Task.CompletedTask;
                result = this.CurrentSong;
                this.HasChanges = true;
            }

            return result;
        }

        public async Task<Song> LoadSong() {
            Song result = null;

            if (this.AskToSave() != MessageBoxResult.Cancel && this._dialogService.ShowFileBrowser(SongFileFilter, out var path)) {
                this._currentSongPath = path;
                this.CurrentSong = await Task.Run(() => Serializer.Instance.Deserialize<Song>(path));
                result = this.CurrentSong;
            }

            return result;
        }

        public async Task<bool> SaveSong() {
            var result = false;

            if (string.IsNullOrEmpty(this._currentSongPath)) {
                result = await this.SaveSongAs();
            }
            else {
                await Task.Run(() => Serializer.Instance.Serialize(this.CurrentSong, this._currentSongPath));
                result = true;
                this.HasChanges = false;
            }

            return result;
        }

        public async Task<bool> SaveSongAs() {
            var result = false;

            if (this._dialogService.ShowSaveFileBrowser(SongFileFilter, out var path)) {
                this._currentSongPath = path;
                await Task.Run(() => Serializer.Instance.Serialize(this.CurrentSong, path));
                result = true;
                this.HasChanges = false;
            }

            return result;
        }

        private MessageBoxResult AskToSave() {
            var result = MessageBoxResult.None;
            if (this.HasChanges && this.CurrentSong != null) {
                this._dialogService.ShowYesNoCancelMessageBox("Save Song", "Would you like to save the currently open song first?");
            }

            return result;
        }
    }
}