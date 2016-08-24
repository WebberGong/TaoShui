using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Dto
{
    [Table("WebSiteSetting")]
    public class WebSiteSettingDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "网站名不能为空")]
        [StringLength(50, ErrorMessage = "网站名不能大于50字符")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "网站地址不能为空")]
        [StringLength(100, ErrorMessage = "网站地址不能大于100字符")]
        public string Url { get; set; }

        [Range(1, 10, ErrorMessage = "验证码长度必须介于1至10字符之间")]
        public int CaptchaLength { get; set; }

        [Range(5, 60, ErrorMessage = "登录超时时间必须介于5至60秒之间")]
        public int LoginTimeOut { get; set; }

        [Range(1, 60, ErrorMessage = "抓取数据时间间隔必须介于1至60秒之间")]
        public int GrabDataInterval { get; set; }
    }
}