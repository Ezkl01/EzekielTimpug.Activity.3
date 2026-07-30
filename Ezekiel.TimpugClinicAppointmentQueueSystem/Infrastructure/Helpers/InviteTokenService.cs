using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


namespace Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Helpers;


public class InviteTokenService
{  
    
        private readonly IConfiguration _configuration;

        public InviteTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateInviteToken(Guid userId)
        {
            var secretKey = _configuration["Jwt:InviteSecret"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("purpose", "invite")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
            public InviteToken? ValidateInviteToken(string token)
            {
                var secretKey = _configuration["Jwt:InviteSecret"];
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
                    var purposeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "purpose");

                    if (userIdClaim == null || purposeClaim == null || purposeClaim.Value != "invite")
                    {
                        return null;
                    }

                    return new InviteToken
                    {
                        UserId = Guid.Parse(userIdClaim.Value)
                    };
                }
                catch
                {
                    return null;
                }
            }
}

public class InviteToken
{
    public Guid UserId { get; set; }
}
    
