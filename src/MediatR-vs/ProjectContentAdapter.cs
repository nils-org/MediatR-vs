using EnvDTE;

using MediatRvs.Extensions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Collections.Generic;
using System.Linq;
using MediatRvs.Models;
using CodeClass = EnvDTE.CodeClass;

namespace MediatRvs
{
    public class ProjectContentAdapter
    {
        public IEnumerable<MediatrProject> GetMessages()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var service = (DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SDTE));
            var projects = service.Solution.Projects.Cast<Project>();
            foreach (var project in projects)
            {
                Logger.Log("Crawling Project: " + project.FullName);
                var projectItems = GetAllProjectItems(project).ToList();

                var elements = new List<MediatrElement>();
                foreach (var item in projectItems)
                {
                    try
                    {
                        var itemElements =
                            GetAllElements(item)
                                .Select(e =>
                                {
                                    ThreadHelper.ThrowIfNotOnUIThread();
                                    return new MediatrElement
                                    {
                                        Name = e.Name,
                                        FullName = e.FullName,
                                        ElementType = GetElementType(e),
                                        ProjectItem = item,
                                        CodeElement = e
                                    };
                                })
                                .Where(x => x.ElementType != MediatrElementType.Unknown)
                                .OrderBy(x => x.Name);


                        elements.AddRange(itemElements);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e);
                    }
                }

                yield return new MediatrProject
                {
                    Name = project.Name,
                    Path = project.FullName,
                    Elements = elements
                };
            }
        }

        private MediatrElementType GetElementType(CodeElement initialElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var stack = new Stack<CodeElement>();
            stack.Push(initialElement);

            Logger.Log("checking: " + initialElement.FullName);

            while (stack.Count > 0)
            {
                var elm = stack.Pop();
                var fullName = elm.FullName;
                Logger.Log(" - " + fullName);

                if (!elm.IsCodeType)
                {
                    // then, why are we here?!
                    return MediatrElementType.Unknown;
                }

                var codeType = (CodeType)elm;
                if (codeType.IsDerivedFrom["MediatR.IRequest"]
                    || codeType.IsDerivedFrom["MediatR.IBaseRequest"])
                {
                    return MediatrElementType.Request;
                }

                if (codeType.IsDerivedFrom["MediatR.INotification"])
                {
                    return MediatrElementType.Notification;
                }

                if (fullName.StartsWith("MediatR.IRequest<", StringComparison.OrdinalIgnoreCase))
                {
                    return MediatrElementType.Request;
                }

                if (fullName.StartsWith("MediatR.INotificationHandler<", StringComparison.OrdinalIgnoreCase))
                {
                    return MediatrElementType.NotificationHandler;
                }

                if (fullName.StartsWith("MediatR.IRequestHandler<", StringComparison.OrdinalIgnoreCase))
                {
                    return MediatrElementType.RequestHandler;
                }

                if (codeType is CodeClass codeClass)
                {
                    codeClass.Bases.EnsureNotNull<CodeElement>().ToList().ForEach(x => stack.Push(x));
                    codeClass.ImplementedInterfaces.EnsureNotNull<CodeElement>().ToList().ForEach(x => stack.Push(x));
                }

                if (codeType is CodeInterface codeInterface)
                {
                    codeInterface.Bases.EnsureNotNull<CodeElement>().ToList().ForEach(x => stack.Push(x));
                }
            }

            return MediatrElementType.Unknown;
        }


        private IEnumerable<CodeElement> GetAllElements(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var stack = new Stack<CodeElement>();
            var fileCode = item.FileCodeModel;
            if (fileCode == null)
            {
                return Enumerable.Empty<CodeElement>();
            }

            var codeElements = fileCode.CodeElements;
            if (codeElements == null)
            {
                return Enumerable.Empty<CodeElement>();
            }

            codeElements.Cast<CodeElement>().ToList().ForEach(x => stack.Push(x));
            var result = new List<CodeElement>();

            while (stack.Count > 0)
            {
                var element = stack.Pop();
                try
                {
                    if (element.Kind == vsCMElement.vsCMElementClass ||
                        element.Kind == vsCMElement.vsCMElementInterface)
                    {
                        result.Add(element);
                    }

                    if (element.Kind == vsCMElement.vsCMElementNamespace ||
                       element.Kind == vsCMElement.vsCMElementClass)
                    {
                        element.Children.EnsureNotNull<CodeElement>().ToList().ForEach(x => stack.Push(x));
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }

            return result;
        }

        private IEnumerable<ProjectItem> GetAllProjectItems(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var stack = new Stack<ProjectItem>();
            project.ProjectItems.EnsureNotNull<ProjectItem>().ToList().ForEach(x => stack.Push(x));

            var result = new List<ProjectItem>();
            while (stack.Count > 0)
            {
                try
                {
                    var item = stack.Pop();
                    result.Add(item);

                    item.ProjectItems.EnsureNotNull<ProjectItem>().ToList().ForEach(x => stack.Push(x));
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }

            return result;
        }
    }
}
