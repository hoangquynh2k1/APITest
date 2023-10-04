using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APITest.Models
{
    public class Tag
    {
        [Key]
        [StringLength(20)]
        public string TagId { set; get; }
        [Column(TypeName = "ntext")]
        public string Content { set; get; }
    }
}
