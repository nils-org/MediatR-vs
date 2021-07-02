using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using MediatRvs.Models;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace MediatRvs
{
    /// <summary>
    /// Interaction logic for MediatrToolWindowControl.
    /// </summary>
    public partial class MediatrToolWindowControl : UserControl, INotifyPropertyChanged
    {
        private MediatrProject[] _projects;
        private ICommand _refreshCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediatrToolWindowControl"/> class.
        /// </summary>
        public MediatrToolWindowControl()
        {
            InitializeComponent();
            DataContext = this;
            RefreshCommand = new DelegateCommand(LoadProjects);
            ThreadHelper.JoinableTaskFactory.Run(async () => await InitLoadProjectsAsync());
        }

        public MediatrProject[] Projects
        {
            get => _projects;
            set
            {
                _projects = value; 
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand
        {
            get => _refreshCommand;
            set
            {
                _refreshCommand = value;
                OnPropertyChanged();
            }
        }

        private async Task InitLoadProjectsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            LoadProjects();
        }

        private void LoadProjects()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var adapter = new ProjectContentAdapter();
                try
                {
                    var projects = adapter.GetMessages().ToArray();
                    Projects = projects;
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
