using System.ComponentModel.DataAnnotations;

namespace BackendNetAD.WebAPI.DTOs
{
    public class UserLoginRequestDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MinLength(8, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MinLength(8, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres")]
        public required string Password { get; set; }
    }
}
