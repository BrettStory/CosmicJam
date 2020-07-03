namespace CosmicJam.Editor.ViewModels {

    using CosmicJam.Library.Services;
    using GalaSoft.MvvmLight.CommandWpf;
    using Macabre2D.Framework;
    using Macabre2D.Wpf.Common.Services;
    using System.Windows.Input;

    public sealed class MainWindowViewModel : NotifyPropertyChanged {
        private readonly RelayCommand _redoCommand;
        private readonly RelayCommand _saveAsCommand;
        private readonly RelayCommand _saveCommand;
        private readonly ISongService _songService;
        private readonly RelayCommand _undoCommand;
        private readonly IUndoService _undoService;

        public MainWindowViewModel(
            IBusyService busyService,
            ISongService songService,
            IUndoService undoService) {
            this.BusyService = busyService;
            this._songService = songService;
            this._undoService = undoService;

            this.LoadCommand = new RelayCommand(async () => await this._songService.LoadSong());
            this.NewCommand = new RelayCommand(async () => await this._songService.CreateSong());
            this._saveCommand = new RelayCommand(async () => await this._songService.SaveSong(), () => this._songService.HasChanges && this._songService.CurrentSong != null);
            this._saveAsCommand = new RelayCommand(async () => await this._songService.SaveSongAs(), () => this._songService.CurrentSong != null);
            this._undoCommand = new RelayCommand(() => this._undoService.Undo(), () => this._undoService.CanUndo);
            this._redoCommand = new RelayCommand(() => this._undoService.Redo(), () => this._undoService.CanRedo);

            this._songService.PropertyChanged += this.SongService_PropertyChanged;
            this._undoService.PropertyChanged += this.UndoService_PropertyChanged;
        }

        public IBusyService BusyService { get; }

        public ICommand LoadCommand { get; }

        public ICommand NewCommand { get; }

        public ICommand RedoCommand {
            get {
                return this._redoCommand;
            }
        }

        public ICommand SaveAsCommand {
            get {
                return this._saveAsCommand;
            }
        }

        public ICommand SaveCommand {
            get {
                return this._saveCommand;
            }
        }

        public ICommand UndoCommand {
            get {
                return this._undoCommand;
            }
        }

        private void SongService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this._songService.CurrentSong)) {
                this._saveCommand.RaiseCanExecuteChanged();
                this._saveAsCommand.RaiseCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(this._songService.HasChanges)) {
                this._saveCommand.RaiseCanExecuteChanged();
            }
        }

        private void UndoService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this._undoService.CanRedo)) {
                this._redoCommand.RaiseCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(this._undoService.CanUndo)) {
                this._undoCommand.RaiseCanExecuteChanged();
            }
        }
    }
}