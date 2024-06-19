using AngularAuth1.Context;
using AngularAuth1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using AngularAuth1.Helpers;

namespace AngularAuth1.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly AppDbContext _authContext;
    public UserController(AppDbContext context)
    {
      _authContext = context;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] User userObj)
    {
      if (userObj == null)
        return BadRequest();

      var user = await _authContext.Users
          .FirstOrDefaultAsync(x => x.Email == userObj.Email); 

      if (user == null)
        return NotFound(new { Message = "User not found!" });

      if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
      {
        return BadRequest(new { Message = "Password is Incorrect" });
      }

     /* user.Token = CreateJwt(user);
      var newAccessToken = user.Token;
      var newRefreshToken = CreateRefreshToken();
      user.RefreshToken = newRefreshToken;
      user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);
      await _authContext.SaveChangesAsync();

      return Ok(new TokenApiDto()
      {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken
      });*/
      return Ok(new
      {
        Message = "Login Success!"
      });
    }

    [HttpPost("register")]
    public async Task<IActionResult> /*AddUser*/RegisterUser([FromBody] User userObj)
    {
      if (userObj == null)
        return BadRequest();

      // check email
      if (await CheckEmailExistAsync(userObj.Email))
        return BadRequest(new { Message = "Email Already Exist" });

      //check username
      if (await CheckUsernameExistAsync(userObj.Name))
        return BadRequest(new { Message = "Username Already Exist" });

      var passMessage = CheckPasswordStrength(userObj.Password);
      if (!string.IsNullOrEmpty(passMessage))
        return BadRequest(new { Message = passMessage.ToString() });

      userObj.Password = PasswordHasher.HashPassword(userObj.Password);
      userObj.Role = "User";
      userObj.Token = "";
      await _authContext.AddAsync(userObj);
      await _authContext.SaveChangesAsync();
      /* return Ok(new
       {
         Status = 200,
         Message = "User Added!"
       });*/
      return Ok(new
      {
        Message = "User Resgistered!"
      });
    
  }

    private Task<bool> CheckEmailExistAsync(string? email)
        => _authContext.Users.AnyAsync(x => x.Email == email);

    private Task<bool> CheckUsernameExistAsync(string? name)
        => _authContext.Users.AnyAsync(x => x.Name == name);

    private static string CheckPasswordStrength(string pass)
    {
      StringBuilder sb = new StringBuilder();
      if (pass.Length < 9)
        sb.Append("Minimum password length should be 8" + Environment.NewLine);
      if (!(Regex.IsMatch(pass, "[a-z]") && Regex.IsMatch(pass, "[A-Z]") && Regex.IsMatch(pass, "[0-9]")))
        sb.Append("Password doit etre aphanumerique" + Environment.NewLine);
      if (!Regex.IsMatch(pass, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
        sb.Append("Password should contain special charcter" + Environment.NewLine);
      return sb.ToString();
    }

   /* private string CreateJwt(User user)
    {
      var jwtTokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes("veryverysceret.....");
      var identity = new ClaimsIdentity(new Claim[]
      {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.Username}")
      });

      var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = identity,
        Expires = DateTime.Now.AddSeconds(10),
        SigningCredentials = credentials
      };
      var token = jwtTokenHandler.CreateToken(tokenDescriptor);
      return jwtTokenHandler.WriteToken(token);
    }

    private string CreateRefreshToken()
    {
      var tokenBytes = RandomNumberGenerator.GetBytes(64);
      var refreshToken = Convert.ToBase64String(tokenBytes);

      var tokenInUser = _authContext.Users
          .Any(a => a.RefreshToken == refreshToken);
      if (tokenInUser)
      {
        return CreateRefreshToken();
      }
      return refreshToken;
    }

    private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
    {
      var key = Encoding.ASCII.GetBytes("veryverysceret.....");
      var tokenValidationParameters = new TokenValidationParameters
      {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = false
      };
      var tokenHandler = new JwtSecurityTokenHandler();
      SecurityToken securityToken;
      var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
      var jwtSecurityToken = securityToken as JwtSecurityToken;
      if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        throw new SecurityTokenException("This is Invalid Token");
      return principal;

    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<User>> GetAllUsers()
    {
      return Ok(await _authContext.Users.ToListAsync());
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
    {
      if (tokenApiDto is null)
        return BadRequest("Invalid Client Request");
      string accessToken = tokenApiDto.AccessToken;
      string refreshToken = tokenApiDto.RefreshToken;
      var principal = GetPrincipleFromExpiredToken(accessToken);
      var username = principal.Identity.Name;
      var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Username == username);
      if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        return BadRequest("Invalid Request");
      var newAccessToken = CreateJwt(user);
      var newRefreshToken = CreateRefreshToken();
      user.RefreshToken = newRefreshToken;
      await _authContext.SaveChangesAsync();
      return Ok(new TokenApiDto()
      {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken,
      });
    }*/
  }
}

