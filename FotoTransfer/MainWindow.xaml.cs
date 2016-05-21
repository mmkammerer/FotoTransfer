using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace FotoTransfer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MainWindow"/>.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Drag-and-drop handler for drag over event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The drag event arguments.</param>
        private void OnDragOver(object sender, DragEventArgs e)
        {
            var viewModel = (ViewModel)this.DataContext;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Drag-and-drop handler for drop event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The drag event arguments.</param>
        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                // The view model is stuffed into the DataContext of the main window.
                // I wonder how it got there? Look into App.xaml.cs.
                var viewModel = (ViewModel)this.DataContext;
                viewModel.DropFiles(droppedFiles);
            }
        }
    }
}
