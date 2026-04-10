using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos.Request.Authenication
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập email đăng nhập")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Nhập mật khẩu")]
        public string Password { get; set; }
    }
}
