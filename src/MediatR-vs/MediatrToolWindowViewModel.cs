using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MediatRvs.Models;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace MediatRvs
{
    public class MediatrToolWindowViewModel: INotifyPropertyChanged
    {
        private MediatrProject[] _projects;
        private ICommand _refreshCommand;

        public MediatrToolWindowViewModel()
        {
            RefreshCommand = new DelegateCommand(() => LoadProjectsAsync().FireAndForget());
            LoadProjectsAsync().FireAndForget();
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

        private async Task LoadProjectsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var adapter = new ProjectContentAdapter();
                try
                {
                    var elements = adapter.GetMediatRContent();
                    Projects = elements.ToArray();
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
