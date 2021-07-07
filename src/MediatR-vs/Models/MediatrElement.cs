using EnvDTE;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

using System.Windows.Input;

namespace MediatRvs.Models
{
    public class MediatrElement
    {
        public string Name { get; set; }

        public MediatrElementType ElementType { get; set; }

        public ICommand OpenDocument => new DelegateCommand(OpenInEditor);

        public ProjectItem ProjectItem { get; set; }

        public CodeElement CodeElement { get; set; }

        public string FullName { get; set; }

        private void OpenInEditor()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var window = ProjectItem.Open(Constants.vsViewKindCode);
                window.Visible = true;
                if (!(window.Document.Object("TextDocument") is TextDocument textDoc))
                {
                    return;
                }

                CodeElement.StartPoint.TryToShow(vsPaneShowHow.vsPaneShowAsIs, CodeElement.StartPoint);
                var selection = (TextSelection)window.Document.Selection;
                selection.MoveToPoint(CodeElement.StartPoint);
            }).FireAndForget();
        }
    }
}
