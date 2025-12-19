namespace cima.Blazor.Client.Services;

/// <summary>
/// Service that caches the MustChangePassword check result 
/// to avoid redundant API calls on each navigation.
/// Registered as Scoped so it persists for the user's session.
/// </summary>
public class MustChangePasswordStateService
{
    private bool _hasChecked = false;
    private bool _mustChangePassword = false;

    /// <summary>
    /// Whether the check has already been performed this session.
    /// </summary>
    public bool HasChecked => _hasChecked;

    /// <summary>
    /// The cached result of the MustChangePassword check.
    /// Only valid if HasChecked is true.
    /// </summary>
    public bool MustChangePassword => _mustChangePassword;

    /// <summary>
    /// Records the result of the MustChangePassword check.
    /// </summary>
    public void SetResult(bool mustChangePassword)
    {
        _mustChangePassword = mustChangePassword;
        _hasChecked = true;
    }

    /// <summary>
    /// Resets the cached state. Call this after the user changes their password
    /// or on logout.
    /// </summary>
    public void Reset()
    {
        _hasChecked = false;
        _mustChangePassword = false;
    }
}
