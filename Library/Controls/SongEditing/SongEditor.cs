namespace CosmicJam.Library.Controls.SongEditing {

    using CosmicJam.Library.Services;
    using Macabre2D.Framework;
    using Macabre2D.Wpf.MonoGameIntegration;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Windows;
    using Point = Microsoft.Xna.Framework.Point;

    public class SongEditor : MonoGameViewModel, IGame {
        public const string SpriteSheetPath = "PianoRollSpriteSheet";
        private static readonly Point BlackPressedKeySpriteLocation = new Point(0, 32);
        private static readonly Point BlackUnpressedKeySpriteLocation = new Point(0, 0);
        private static readonly Point PianoKeySpriteSize = new Point(32, 16);
        private static readonly Point WhitePressedKeySpriteLocation = new Point(0, 48);
        private static readonly Point WhiteUnpressedKeySpriteLocation = new Point(0, 16);
        private readonly Sprite _blackKeyPressed;
        private readonly Sprite _blackKeyUnpressed;
        private readonly Camera _camera;
        private readonly ISongService _songService;
        private readonly Sprite _whiteKeyPressed;
        private readonly Sprite _whiteKeyUnpressed;
        private InputState _currentInputState;
        private FrameTime _frameTime;
        private bool _isContentLoaded = false;
        private bool _isInitialized = false;
        private LiveSongPlayer _liveSongPlayer;
        private PianoComponent _pianoComponent;
        private Point _viewportSize;

        public SongEditor(ISongService songService) : base() {
            this._songService = songService;

            this.Settings = new GameSettings() {
                PixelsPerUnit = 16
            };

            GameSettings.Instance = this.Settings;
            MacabreGame.Instance = this;

            this.CurrentScene = new Scene {
                BackgroundColor = Color.Black
            };

            this._camera = this.CurrentScene.AddChild<Camera>();
            this._camera.ViewHeight = 36f;
            this._camera.OffsetSettings.OffsetType = PixelOffsetType.BottomLeft;
            this._camera.LocalPosition = Vector2.Zero;

            this.AssetManager.SetMapping(Guid.NewGuid(), SpriteSheetPath);
            var spriteSheetId = this.AssetManager.GetId(SpriteSheetPath);
            this._blackKeyUnpressed = new Sprite(spriteSheetId, BlackUnpressedKeySpriteLocation, PianoKeySpriteSize);
            this._blackKeyPressed = new Sprite(spriteSheetId, BlackPressedKeySpriteLocation, PianoKeySpriteSize);
            this._whiteKeyUnpressed = new Sprite(spriteSheetId, WhiteUnpressedKeySpriteLocation, PianoKeySpriteSize);
            this._whiteKeyPressed = new Sprite(spriteSheetId, WhitePressedKeySpriteLocation, PianoKeySpriteSize);
        }

        public event EventHandler<double> GameSpeedChanged;

        public event EventHandler<Point> ViewportSizeChanged;

        public IAssetManager AssetManager { get; } = new AssetManager();

        public IScene CurrentScene { get; }

        public double GameSpeed {
            get {
                return 1f;
            }

            set {
                this.GameSpeedChanged.SafeInvoke(this, 1f);
            }
        }

        public GraphicsSettings GraphicsSettings { get; } = new GraphicsSettings();

        public bool IsDesignMode {
            get {
                return true;
            }
        }

        public ISaveDataManager SaveDataManager { get; } = new EmptySaveDataManager();

        public IGameSettings Settings { get; }

        public bool ShowGrid { get; internal set; } = true;

        public bool ShowSelection { get; internal set; } = true;

        public SpriteBatch SpriteBatch { get; private set; }

        public Point ViewportSize {
            get {
                return this._viewportSize;
            }
        }

        public override void Draw(GameTime gameTime) {
            if (this._isInitialized && this._isContentLoaded) {
                this.GraphicsDevice.Clear(this.CurrentScene.BackgroundColor);
                this.CurrentScene.Draw(this._frameTime);
            }
        }

        public void Exit() {
            return;
        }

        public override void Initialize(MonoGameKeyboard keyboard, MonoGameMouse mouse) {
            base.Initialize(keyboard, mouse);
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.AssetManager.Initialize(this.Content);
            this._songService.PropertyChanged += this.SongService_PropertyChanged;
            this.ResetPianoRoll();
            this.CurrentScene.Initialize();
            this._isInitialized = true;
        }

        public override void LoadContent() {
            this.CurrentScene.LoadContent();
            this._isContentLoaded = true;
        }

        public void ResetCamera() {
            // This probably seems weird, but it resets the view height which causes the view matrix
            // and bounding area to be reevaluated.
            this._camera.ViewHeight += 1;
            this._camera.ViewHeight -= 1;
        }

        public void SaveAndApplyGraphicsSettings() {
            return;
        }

        public override void SizeChanged(object sender, SizeChangedEventArgs e) {
            this._viewportSize = new Point(Convert.ToInt32(e.NewSize.Width), Convert.ToInt32(e.NewSize.Height));
            this.ViewportSizeChanged.SafeInvoke(this, this._viewportSize);

            if (e.NewSize.Width > e.PreviousSize.Width || e.NewSize.Height > e.PreviousSize.Height) {
                this.ResetCamera();
            }
        }

        public override void Update(GameTime gameTime) {
            this._currentInputState = new InputState(MonoGameMouse.Instance.GetState(), MonoGameKeyboard.Instance.GetState(), this._currentInputState);
            this._frameTime = new FrameTime(gameTime, this.GameSpeed);
            this.CurrentScene.Update(this._frameTime, this._currentInputState);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                this._songService.PropertyChanged -= this.SongService_PropertyChanged;
            }
        }

        private void ResetPianoRoll() {
            if (this._pianoComponent != null) {
                this.CurrentScene.RemoveComponent(this._pianoComponent);
            }

            FrameworkDispatcher.Update();
            this._liveSongPlayer = new LiveSongPlayer(this._songService.CurrentSong);
            this._pianoComponent = new PianoComponent(this._liveSongPlayer, this._whiteKeyUnpressed, this._whiteKeyPressed, this._blackKeyUnpressed, this._blackKeyPressed);
            this.CurrentScene.AddChild(this._pianoComponent);
        }

        private void SongService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(ISongService.CurrentSong)) {
                this.ResetPianoRoll();
            }
        }
    }
}