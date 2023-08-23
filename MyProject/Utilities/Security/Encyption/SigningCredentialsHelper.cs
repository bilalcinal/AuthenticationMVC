using Microsoft.IdentityModel.Tokens;

namespace MyProject.Utilities.Security.Encyption;
public class SigningCredentialsHelper
{
	public static SigningCredentials CreateSigningCredentials(SecurityKey securityKey)
	{
		return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
	}
}