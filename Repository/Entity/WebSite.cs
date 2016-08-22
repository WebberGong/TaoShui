using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entity
{
    public class WebSite
    {
        [Required]
        [MaxLength(50)]
        public string LoginName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Password { get; set; }
    }
}
