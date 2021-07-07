using System;
using System.Windows.Controls;

namespace MediatRvs
{
    /// <summary>
    /// Interaction logic for MediatrToolWindowControl.
    /// </summary>
    public partial class MediatrToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediatrToolWindowControl"/> class.
        /// </summary>
        public MediatrToolWindowControl()
        {
            InitializeComponent();
            DataContext = new MediatrToolWindowViewModel();
        }

    }
}
