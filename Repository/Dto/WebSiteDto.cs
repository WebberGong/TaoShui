﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Dto
{
    [Table("WebSite")]
    public class WebSiteDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "用户名不能为空")]
        [StringLength(50, ErrorMessage = "用户名不能大于50字符")]
        public string LoginName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "密码不能为空")]
        [StringLength(50, ErrorMessage = "密码不能大于50字符")]
        public string Password { get; set; }

        [Required]
        public long SettingId { get; set; }
    }
}