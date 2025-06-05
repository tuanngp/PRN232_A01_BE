using System.Text.Json.Serialization;

namespace BusinessObject
{
    public class NewsArticleTag
    {
        public int NewsArticleId { get; set; }

        [JsonIgnore]
        public virtual NewsArticle? NewsArticle { get; set; }

        public int TagId { get; set; }

        public virtual Tag? Tag { get; set; }
    }
}
