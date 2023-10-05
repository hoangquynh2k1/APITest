using System.ComponentModel.DataAnnotations.Schema;
namespace APITest.Models
{
    [Index(nameof(ArticleId))]
    public class Article
    {
        [Key]
        public int ArticleId { set; get; }

        [StringLength(100)]
        public string Title { set; get; }

    }
}
