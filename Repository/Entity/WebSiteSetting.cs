using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Migrations.Model;

namespace Repository.Entity
{
    [Table("WebSiteSetting")]
    public class WebSiteSetting
    {
        public WebSiteSetting()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        [MaxLength(50)]
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Url { get; set; }

        [Required]
        public int CaptchaLength { get; set; }

        [Required]
        public int LoginTimeOut { get; set; }

        [Required]
        public int GrabDataInterval { get; set; }
    }
}