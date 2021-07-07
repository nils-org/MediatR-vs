using Microsoft.VisualStudio.Shell;

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging.Interop;

namespace MediatRvs
{
    [Guid("361d6581-b81e-4c41-a9d1-b8cf5ac457a9")]
    public class MediatrToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediatrToolWindow"/> class.
        /// </summary>
        public MediatrToolWindow() : base(null)
        {
            Caption = Names.ToolWindow.Title;
            BitmapImageMoniker = new ImageMoniker
            {
                Guid = Guid.Parse("77d336fb-b0cf-40c5-81d9-8a12e85018f1"),
                Id = 4
            };

            Content = new MediatrToolWindowControl();
        }
    }
}
