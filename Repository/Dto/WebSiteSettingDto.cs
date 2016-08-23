using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Dto
{
    [Table("WebSiteSetting")]
    public class WebSiteSettingDto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100)]
        public string Url { get; set; }

        [Required]
        public int CaptchaLength { get; set; }

        [Required]
        public int LoginTimeOut { get; set; }

        [Required]
        public int GrabDataInterval { get; set; }
    }
}