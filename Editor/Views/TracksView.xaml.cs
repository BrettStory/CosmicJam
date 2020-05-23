namespace CosmicJam.Editor.Views {

    using Macabre2D.Wpf.Common;
    using CosmicJam.Editor.ViewModels;
    using System.Windows.Controls;

    public partial class TracksView : UserControl {

        public TracksView() {
            this.DataContext = ViewContainer.Resolve<TracksViewModel>();
            this.InitializeComponent();
        }
    }
}