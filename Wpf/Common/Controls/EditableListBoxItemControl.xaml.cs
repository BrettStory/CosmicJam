﻿namespace Macabre2D.Wpf.Common.Controls {

    using Macabre2D.Wpf.Common;
    using Macabre2D.Wpf.Common.Models;
    using Macabre2D.Wpf.Common.Services;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class EditableListBoxItemControl : UserControl, INotifyPropertyChanged {

        public static readonly DependencyProperty AllowUndoProperty = DependencyProperty.Register(
            nameof(AllowUndo),
            typeof(bool),
            typeof(EditableListBoxItemControl),
            new PropertyMetadata());

        public static readonly DependencyProperty InvalidTypesProperty = DependencyProperty.Register(
            nameof(InvalidTypes),
            typeof(IEnumerable<Type>),
            typeof(EditableListBoxItemControl),
            new PropertyMetadata(new List<Type>()));

        public static readonly DependencyProperty IsFileNameProperty = DependencyProperty.Register(
                    nameof(IsFileName),
            typeof(bool),
            typeof(EditableListBoxItemControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty ShouldSetTextAutomaticallyProperty = DependencyProperty.Register(
            nameof(ShouldSetTextAutomatically),
            typeof(bool),
            typeof(EditableListBoxItemControl),
            new PropertyMetadata(true));

        public static readonly DependencyProperty TextChangedCommandProperty = DependencyProperty.Register(
            nameof(TextChangedCommand),
            typeof(ICommand),
            typeof(EditableListBoxItemControl),
            new PropertyMetadata());

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(EditableListBoxItemControl),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

        public static readonly DependencyProperty ValidTypesProperty = DependencyProperty.Register(
            nameof(ValidTypes),
            typeof(IEnumerable<Type>),
            typeof(EditableListBoxItemControl),
            new PropertyMetadata(new List<Type>()));

        private readonly ICommonDialogService _dialogService;
        private readonly IUndoService _undoService;
        private bool _isEditing;

        public EditableListBoxItemControl() {
            this._dialogService = ViewContainer.Resolve<ICommonDialogService>();
            this._undoService = ViewContainer.Resolve<IUndoService>();
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AllowUndo {
            get { return (bool)this.GetValue(AllowUndoProperty); }
            set { this.SetValue(AllowUndoProperty, value); }
        }

        public IEnumerable<Type> InvalidTypes {
            get { return (IEnumerable<Type>)this.GetValue(InvalidTypesProperty); }
            set { this.SetValue(InvalidTypesProperty, value); }
        }

        public bool IsEditing {
            get {
                return this._isEditing;
            }

            set {
                if (this._isEditing != value) {
                    this._isEditing = value;

                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsEditing)));
                }
            }
        }

        public bool IsFileName {
            get { return (bool)this.GetValue(IsFileNameProperty); }
            set { this.SetValue(IsFileNameProperty, value); }
        }

        public bool ShouldSetTextAutomatically {
            get { return (bool)this.GetValue(ShouldSetTextAutomaticallyProperty); }
            set { this.SetValue(ShouldSetTextAutomaticallyProperty, value); }
        }

        public string Text {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public ICommand TextChangedCommand {
            get { return (ICommand)this.GetValue(TextChangedCommandProperty); }
            set { this.SetValue(TextChangedCommandProperty, value); }
        }

        public IEnumerable<Type> ValidTypes {
            get { return (IEnumerable<Type>)this.GetValue(ValidTypesProperty); }
            set { this.SetValue(ValidTypesProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is EditableListBoxItemControl control) {
                control._editableTextBox.Text = control.GetEditableText(e.NewValue as string);
            }
        }

        private bool CanEditListBoxItem(ListBoxItem listBoxItem) {
            return listBoxItem?.DataContext != null &&
                listBoxItem.IsSelected &&
                (this.InvalidTypes == null || !this.InvalidTypes.Contains(listBoxItem.DataContext.GetType())) &&
                (this.ValidTypes == null || !this.ValidTypes.Any() || this.ValidTypes.Contains(listBoxItem.DataContext.GetType()));
        }

        private void CommitNewText(string oldText, string newText) {
            if (this.IsFileName && !FileHelper.IsValidFileName(newText)) {
                this._dialogService.ShowWarningMessageBox("Invalid File Name", $"'{newText}' contains invalid characters.");
            }
            else {
                if (this.TextChangedCommand?.CanExecute(newText) == true) {
                    this.TextChangedCommand.Execute(newText);
                }

                if (this.ShouldSetTextAutomatically) {
                    if (this.AllowUndo) {
                        var undoCommand = new UndoCommand(() => {
                            this.SetText(newText);
                        }, () => {
                            this.SetText(oldText);
                        });

                        this._undoService.Do(undoCommand);
                    }
                    else {
                        this.SetText(newText);
                    }
                }
            }

            this.IsEditing = false;
        }

        private string GetEditableText(string originalText) {
            var result = originalText;

            if (this.IsFileName) {
                result = Path.GetFileNameWithoutExtension(originalText);
            }

            return result;
        }

        private void ListBoxItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (sender is TextBox textBox && textBox.IsVisible) {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        private void ListBoxItem_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                var newText = this.IsFileName ? $"{this._editableTextBox.Text}{Path.GetExtension(this.Text)}" : this._editableTextBox.Text;
                this.CommitNewText(this.Text, newText);
            }
            else if (e.Key == Key.Escape) {
                if (sender is TextBox textBox) {
                    textBox.Text = this.GetEditableText(this.Text);
                    this.IsEditing = false;
                }
            }
        }

        private void ListBoxItem_LostFocus(object sender, RoutedEventArgs e) {
            this.IsEditing = false;
            this._editableTextBox.Text = this.GetEditableText(this.Text);
        }

        private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var listBoxItem = (e.OriginalSource as DependencyObject)?.FindAncestor<ListBoxItem>();
            if (this.CanEditListBoxItem(listBoxItem)) {
                this._editableTextBox.Text = this.GetEditableText(this.Text);
                this.IsEditing = true;
                e.Handled = true;
            }
        }

        private void SetText(string text) {
            this.Dispatcher.BeginInvoke((Action)(() => {
                this.Text = text;
                this._editableTextBox.Text = this.GetEditableText(text);
            }));
        }
    }
}