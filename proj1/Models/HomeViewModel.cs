using System.Collections.Generic;

namespace proj1.Models
{
    public class HomeViewModel
    {
        public List<News> SliderNews { get; set; } = new List<News>();
        public List<News> LatestNews { get; set; } = new List<News>();
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
