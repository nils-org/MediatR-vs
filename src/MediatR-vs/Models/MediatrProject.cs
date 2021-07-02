using System.Collections.Generic;

namespace MediatRvs.Models
{
    public class MediatrProject
    {
        public string Name { get; set; }

        public IEnumerable<MediatrElement> Elements { get; set; }
        public string Path { get; set; }
    }
}