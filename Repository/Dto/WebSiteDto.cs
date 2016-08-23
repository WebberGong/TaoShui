using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalaSoft.MvvmLight;

namespace Repository.Dto
{
    [Table("WebSite")]
    public class WebSiteDto : ObservableObject
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string LoginName { get; set; }

        [Required]
        [StringLength(50)]
        public string Password { get; set; }

        [Required]
        public long SettingId { get; set; }

        [Required]
        [ForeignKey("SettingId")]
        public virtual WebSiteSettingDto Setting { get; set; }
    }
}