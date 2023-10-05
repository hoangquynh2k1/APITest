using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APITest.Models
{
    public class Tag
    {
        [Key]
        public int TagId { set; get; }

        [Column(TypeName = "MEDIUMTEXT")]
        public string? Content { set; get; }
    }
}
