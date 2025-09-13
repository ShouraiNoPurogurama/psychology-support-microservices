namespace BuildingBlocks.Constants;

public static class MyStrings
{
    private const string RevokedTokensPrefix = "revoked_tokens";

    /// <summary>
    /// Generates the cache key for a revoked JWT.
    /// Format: revoked_tokens:{jti}
    /// </summary>
    /// <param name="jti">The JWT ID claim.</param>
    /// <returns>The formatted cache key.</returns>
    public static string GenerateRevokedTokenKey(string jti)
    {
        return $"{RevokedTokensPrefix}:{jti}";
    }
}