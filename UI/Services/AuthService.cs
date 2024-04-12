using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UI.Models;

namespace UI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
        }
        public async Task<AccountStatus> RegisterAsync(Register model)
        {
            var status = new AccountStatus();

            if (model.Password != model.PasswordConfirm)
            {
                status.StatusCode = 0;
                status.StatusMessage = "Confirm password doesn't math the password";

                return status;
            }

            var userExists = await _userManager.FindByNameAsync(model.UserName);

            if (userExists != null)
            {
                status.StatusCode = 0;
                status.StatusMessage = "User already exist";

                return status;
            }

            IdentityUser user = new IdentityUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                status.StatusCode = 0;
                status.StatusMessage = "User creation failed";

                return status;
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }

            if (await _roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            status.StatusCode = 1;
            status.StatusMessage = "You have registered successfully";

            return status;
        }
        public async Task<AccountStatus> LoginAsync(Login model)
        {
            var status = new AccountStatus();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                status.StatusCode = 0;
                status.StatusMessage = "Invalid Email or Password";

                return status;
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                status.StatusCode = 0;
                status.StatusMessage = "Invalid Email or Password";

                return status;
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, true);

            if (signInResult.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                };

                var jwtToken = CreateToken(user, userRoles);

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                status.Token = jwtToken;
                status.StatusCode = 1;
                status.StatusMessage = "Logged in successfully";
            }
            else if (signInResult.IsLockedOut)
            {
                status.StatusCode = 0;
                status.StatusMessage = "User is locked out";
            }
            else
            {
                status.StatusCode = 0;
                status.StatusMessage = "Error on logging in";
            }

            return status;
        }
        private string CreateToken(IdentityUser user, IList<string> roles)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _config.GetSection("JwtConfig:Secret").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddSeconds(3600),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }


    }
}
