using Azure.Core;
using Hospital_Management_System.Database;
using Hospital_Management_System.Helper;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hospital_Management_System.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly HospitalDbContext _dbContext;
        private readonly Password _password;
        private readonly IConfiguration _configuration;
        public LoginController(ILogger<LoginController> logger, HospitalDbContext dbContext, Password password, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _password = password;
            _configuration = configuration;
        }
       
        public IActionResult LoginView()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckUsernameAndPassword(string username, string password)
        {
            try
            {
                // Check if the employee exists with the given username
                var employee = await _dbContext.Login
                    .FirstOrDefaultAsync(e => e.Username == username);

                if (employee == null)
                {
                    // Username not found
                    return Json(new { success = false, message = "Username not found. Please check your username and try again." });
                }

                // Verify the password (assuming passwords are hashed)
                var decryptedPassword = _password.UnHashPassword(employee.Password);
                if (password != decryptedPassword)
                {
                    // Incorrect password
                    return Json(new { success = false, message = "Incorrect password. Please try again." });
                }

                // Store the username in session
                HttpContext.Session.SetString("UserName", username);

                // Generate the JWT Token
                var token = GenerateJwtToken(employee);
                SetJwtCookie(token);

                // Set the token in an HTTP-only cookie (secure and cannot be accessed via JavaScript)
                HttpContext.Session.SetString("accessToken", token);

                // Return success message along with a redirect URL to the dashboard
                return Json(new { success = true, redirectTo = "/Home/Index", accessToken = token, message="welcome!" });
            }
            catch (Exception ex)
            {
                // Log the exception details and return an error response
                _logger.LogError($"An error occurred: {ex.Message}");
                return Json(new { success = false, message = "Internal server error. Please try again later." });
            }
        }

        private string GenerateJwtToken(Login employee)
        {
            var jwtSettings = _configuration.GetSection("JWT");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, employee.StaffID.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, employee.Username),
                new Claim("role", employee.Role), // Add additional claims as needed
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["ValidIssuer"],
                audience: jwtSettings["ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void SetJwtCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Ensures the cookie is not accessible via JavaScript (prevents XSS)
                Secure = true,   // Only send cookie over HTTPS (make sure this is true in production)
                Expires = DateTime.Now.AddMinutes(60) // Set expiration time (same as token expiration)
            };

            Response.Cookies.Append("jwtToken", token, cookieOptions);
        }
        public IActionResult Logout()
        {
            // Clear the JWT token cookie by setting its expiration to a past date
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1), // Expire the cookie immediately
                HttpOnly = true
            };
            Response.Cookies.Append("jwtToken", "", options); // Overwrite the cookie with an empty value

            // Optionally, clear any session or authentication
            HttpContext.Session.Clear(); // Clear session if applicable

            // Redirect to the login or home page after logout
            return RedirectToAction("LoginView");
        }

    }
}
