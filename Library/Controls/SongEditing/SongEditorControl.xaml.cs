namespace CosmicJam.Library.Controls.SongEditing {

    using Macabre2D.Wpf.Common;
    using System.Windows.Controls;

    public partial class SongEditorControl : UserControl {

        public SongEditorControl() {
            this.InitializeComponent();
        }

        public SongEditor SongEditor { get; } = ViewContainer.Resolve<SongEditor>();
    }
}