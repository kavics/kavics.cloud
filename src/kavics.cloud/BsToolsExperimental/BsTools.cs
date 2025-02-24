using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SenseNet.Configuration;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Search;
using SenseNet.Services.Core.Operations;
using SNCR = SenseNet.ContentRepository;

// ReSharper disable once CheckNamespace
namespace SenseNet.BusinessSolutions.Common;

//TODO: It would be nice to outsource this class to a nuget package commonly used by sensenet BS
public class BsTools
{
    // ReSharper disable once ClassNeverInstantiated.Local
    private class SafeQueriesForTools : ISafeQueryHolder
    {
        public static string UserByEmail => "+TypeIs:@0 +InTree:@1 +Email:@2";
    }

    /// <summary>
    /// Plays the login operation and returns the logged-in user instance with the desired user type.
    /// Returns null, if the user cannot be found by the "login" parameter, or the password does not match
    /// or the found user is not the desired type or not enabled.
    /// The "login" parameter can be email, loginName, or userName (domainName\loginName).
    /// </summary>
    /// <typeparam name="T">Desired user type. Can be SenseNet.ContentRepository.User or any descendant type</typeparam>
    /// <param name="login">Email, loginName or userName (domainName\loginName)</param>
    /// <param name="password">Password</param>
    /// <param name="usersContainerPath">Root path of the search</param>
    /// <param name="httpContext">Current HttpContext</param>
    /// <param name="logger">ILogger instance</param>
    /// <param name="cancel">The token to monitor for cancellation requests.</param>
    /// <returns>A Task that represents the asynchronous operation and wraps the logged-in user or null.</returns>
    public static async Task<T?> LoginByNameOrEmailAsync<T>(string login, string password, string? usersContainerPath, HttpContext httpContext, ILogger logger, CancellationToken cancel) where T : SNCR.User
    {
        // Note: User content has more "name":
        // - Name:          content's name in the Path
        // - DisplayName:   human-readable name
        // - FullName:      Name in human communications
        // - LoginName:     Nickname in the login operation
        // - UserName:      (computed) domainName\loginName (domain is an ancestor of the user content)

        ArgumentNullException.ThrowIfNull(login);
        ArgumentNullException.ThrowIfNull(password);

        // Load user by loginName, userName or email
        SNCR.User snUser;
        using (new SystemAccount())
        {
            // Load by loginName or userName
            snUser = SNCR.User.Load(login);
            if (snUser == null)
            {
                var userType = typeof(T).Name;
                var usersRoot = usersContainerPath ?? RepositoryStructure.ImsFolderPath;
                // Load by email
                var query = ContentQuery.CreateQuery(SafeQueriesForTools.UserByEmail, QuerySettings.AdminSettings,
                    userType, usersRoot, login);
                var userQueryResult = await query.ExecuteAsync(cancel).ConfigureAwait(false);
                // only one hit can be valid
                if (userQueryResult.Count == 1)
                    snUser = (SNCR.User)userQueryResult.Nodes.First();
            }
        }
        if (snUser == null)
            return null;
        if (!snUser.Enabled)
            return null;

        if (snUser is not T loggedInUser)
            return null;

        try
        {
            var userValidationResult = 
                IdentityOperations.ValidateCredentials(default, httpContext, loggedInUser.Username, password);
            if (userValidationResult == null || userValidationResult.Id == 0)
            {
                logger.LogInformation("Login for user {User} is unsuccessful", login);
                return null;
            }
        }
        catch (SenseNetSecurityException ex)
        {
            logger.LogInformation("{Exception} happened during login for user {User}", ex.GetType().Name, login);
            return null;
        }

        return loggedInUser;
    }
}