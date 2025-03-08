using BackendNetAD.WebAPI.Configurations;
using BackendNetAD.WebAPI.DTOs;
using BackendNetAD.WebAPI.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Runtime.Versioning;

namespace BackendNetAD.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ActiveDirectorySettings _settings;

        public AuthController(ILogger<AuthController> logger, IOptions<ActiveDirectorySettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        [EndpointSummary("Realizar el logeo de usuario en AD, metodo 1")]
        [SupportedOSPlatform("windows")]
        [HttpPost("Login1")]
        public IActionResult Login1([FromBody] UserLoginRequestDTO request)
        {
            try
            {
                // Validar usuario con AD
                var resultAD = ValidarUsuarioAD(request.UserName, request.Password);

                // Error de autenticación
                if (resultAD == null)
                {
                    // Registrar log de advertencia
                    _logger.LogWarning("Usuario y/o contraseña incorrecta para el usuario: {UserName}", request.UserName);

                    return BadRequest(new ApiResponse()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Message = "Usuario y/o contraseña incorrectos."
                    });
                }

                // Obtener información del usuario y liberar memoria mediante using
                using var entry = resultAD.GetDirectoryEntry();
                string nombreCompleto = entry.Properties["displayname"].Value?.ToString() ?? "No disponible";
                string departamento = entry.Properties["department"].Value?.ToString() ?? "No disponible";

                // Registrar log de información
                _logger.LogInformation("Usuario {UserName} autenticado con éxito.", request.UserName);

                return Ok(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Message = "Usuario autenticado con éxito.",
                    Data = new { nombreCompleto, departamento }
                });
            }
            catch (Exception ex)
            {
                // Registrar log de error
                _logger.LogError(ex, "Error en el método Login para el usuario {UserName}", request.UserName);

                return StatusCode(500, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "Ocurrió un error en el servidor. Inténtelo más tarde."
                });
            }
        }

        [EndpointSummary("Realizar el logeo de usuario en AD, metodo 2")]
        [SupportedOSPlatform("windows")]
        [HttpPost("Login2")]
        public IActionResult Login2([FromBody] UserLoginRequestDTO request)
        {
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _settings.DomainWSLocal);

                // Verificar si el usuario existe en AD
                using var user = UserPrincipal.FindByIdentity(context, request.UserName);

                if (user == null)
                {
                    // Registrar log de advertencia
                    _logger.LogWarning("Intento de login con usuario inexistente: {UserName}", request.UserName);

                    return NotFound(new ApiResponse()
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        Message = "El usuario no existe."
                    });
                }

                // Validar la contraseña
                bool isValid = context.ValidateCredentials(request.UserName, request.Password);

                if (!isValid)
                {
                    // Registrar log de advertencia
                    _logger.LogWarning("Usuario y/o contraseña incorrecta para el usuario: {UserName}", request.UserName);

                    return Unauthorized(new ApiResponse()
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        IsSuccess = false,
                        Message = "Usuario y/o contraseña incorrectos."
                    });
                }

                // Obtener más información del usuario
                var userData = new
                {
                    NombreCompleto = user.DisplayName ?? "No disponible",
                    Email = user.EmailAddress ?? "No disponible",
                    Telefono = user.VoiceTelephoneNumber ?? "No disponible",
                    Grupos = user.GetAuthorizationGroups().Select(g => g.SamAccountName).ToList()
                };

                // Registrar log de información
                _logger.LogInformation("Usuario {UserName} autenticado con éxito.", request.UserName);

                return Ok(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Message = "Login exitoso.",
                    Data = userData
                });
            }
            catch (Exception ex)
            {
                // Registrar log de error
                _logger.LogError(ex, "Error en el método Login para el usuario {UserName}", request.UserName);

                return StatusCode(500, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "Ocurrió un error en el servidor. Inténtelo más tarde."
                });
            }
        }

        [SupportedOSPlatform("windows")]
        private SearchResult? ValidarUsuarioAD(string UserName, string Password)
        {
            string ruta = "LDAP://" + _settings.DomainWSLocal;
            DirectoryEntry de = new(ruta, UserName, Password);
            DirectorySearcher searcher = new(de);
            SearchResult? searchResult = null;
            searcher.Filter = $"(sAMAccountName={UserName})";
            searchResult = searcher.FindOne();
            return searchResult;
        }
    }
}
