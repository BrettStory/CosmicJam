﻿namespace CosmicJam.Library.Controls.SongEditing {

    using System.Windows.Controls;

    public partial class SongEditorControl : UserControl {

        public SongEditorControl() {
            this.SongEditor = new SongEditor();
            this.InitializeComponent();
        }

        public SongEditor SongEditor { get; }
    }
}