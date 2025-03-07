using BackendNetAD.WebAPI.Configurations;
using BackendNetAD.WebAPI.DTOs;
using BackendNetAD.WebAPI.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace BackendNetAD.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ActiveDirectorySettings _adSettings;

        public AuthController(ILogger<AuthController> logger, IOptions<ActiveDirectorySettings> adSettings)
        {
            _logger = logger;
            _adSettings = adSettings.Value;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLoginRequestDTO model)
        {
            // Instanciamos el modelo de respuesta por cada metodo y evitar errores de concurrencia
            ApiResponse apiResponse = new();

            // Validar el modelo
            if (!ModelState.IsValid)
            {
                apiResponse.StatusCode = HttpStatusCode.BadRequest;
                apiResponse.IsSuccess = false;
                apiResponse.Message = "Error en el modelo.";
                apiResponse.Data = ModelState.Values
                                    .Select(v => v.Errors)
                                    .ToList();
                return BadRequest(apiResponse);
            }

            apiResponse.StatusCode = HttpStatusCode.OK;
            apiResponse.IsSuccess = true;
            apiResponse.Message = "Usuario autenticado con Exito.";
            apiResponse.Data = _adSettings.DomainWSLocal;

            // Log the message
            _logger.LogInformation("Usuario autenticado con Exito.");

            return Ok(apiResponse);
        }
    }
}
