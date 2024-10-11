using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace MessagePush.Commons;

public static class JwtHelper
{
    private static TokenValidationParameters CreateTokenValidationParameters()
    {
        var result = new TokenValidationParameters
        {
            ValidateIssuer = false,

            ValidateAudience = false,

            ValidateIssuerSigningKey = false,

            //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)),
            //comment this and add this line to fool the validation logic
            SignatureValidator = delegate(string token, TokenValidationParameters parameters)
            {
                var jwt = new JwtSecurityToken(token);

                return jwt;
            },

            RequireExpirationTime = true,
            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };

        result.RequireSignedTokens = false;

        return result;
    }
}