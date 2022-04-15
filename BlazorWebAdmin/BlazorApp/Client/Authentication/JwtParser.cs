using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;

namespace Cores.GrpcClient.Authentication
{
    public class JwtParser
    {
        public static IEnumerable<System.Security.Claims.Claim> ParseClaimFromJWT(string jwt)
        {
            var claims = new List<System.Security.Claims.Claim>();
            //
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(jwt);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            //
            ExtractRolesFromJWT(claims, keyValuePairs);
            //
            claims.AddRange(keyValuePairs.Select(kvp => new System.Security.Claims.Claim(kvp.Key, kvp.Value.ToString())));
            //
            return claims;
        }

        public static void ExtractRolesFromJWT(List<System.Security.Claims.Claim> claims, Dictionary<string, object> keyValuePairs)
        {
            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);
            //
            if (roles is not null)
            {
                var parsedRoles = roles.ToString().Trim().TrimStart('[').TrimEnd(']').Split(',');
                foreach (var parsedRole in parsedRoles)
                {
                    claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, parsedRole.Trim('"')));
                }
                //
                keyValuePairs.Remove(ClaimTypes.Role);
            }
        }
        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "=";
                    break;
                case 3:
                    base64 += "==";
                    break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
