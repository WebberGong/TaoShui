using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Dto
{
    [Table("WebSite")]
    public class WebSiteDto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string LoginName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string Password { get; set; }

        [Required]
        public long SettingId { get; set; }
    }
}