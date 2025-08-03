using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using SwipeMyRoof.OSM.Services;

namespace SwipeMyRoof.UI.ViewModels;

/// <summary>
/// View model for OSM authentication
/// </summary>
public class OsmAuthViewModel : ViewModelBase
{
    private readonly IOsmAuthService _authService;
    private string _authUrl = string.Empty;
    private string _verifier = string.Empty;
    private bool _isAuthenticating;
    private bool _isAuthenticated;
    private string? _username;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="authService">OSM authentication service</param>
    public OsmAuthViewModel(IOsmAuthService authService)
    {
        _authService = authService;
        
        StartAuthCommand = ReactiveCommand.CreateFromTask(StartAuthAsync);
        CompleteAuthCommand = ReactiveCommand.CreateFromTask(CompleteAuthAsync);
        SignOutCommand = ReactiveCommand.Create(SignOut);
        
        // Check if we're already authenticated
        IsAuthenticated = _authService.IsAuthenticated();
        Username = _authService.GetUsername();
    }
    
    /// <summary>
    /// Command to start the authentication process
    /// </summary>
    public ICommand StartAuthCommand { get; }
    
    /// <summary>
    /// Command to complete the authentication process
    /// </summary>
    public ICommand CompleteAuthCommand { get; }
    
    /// <summary>
    /// Command to sign out
    /// </summary>
    public ICommand SignOutCommand { get; }
    
    /// <summary>
    /// Authentication URL
    /// </summary>
    public string AuthUrl
    {
        get => _authUrl;
        private set => this.RaiseAndSetIfChanged(ref _authUrl, value);
    }
    
    /// <summary>
    /// OAuth verifier
    /// </summary>
    public string Verifier
    {
        get => _verifier;
        set => this.RaiseAndSetIfChanged(ref _verifier, value);
    }
    
    /// <summary>
    /// Whether authentication is in progress
    /// </summary>
    public bool IsAuthenticating
    {
        get => _isAuthenticating;
        private set => this.RaiseAndSetIfChanged(ref _isAuthenticating, value);
    }
    
    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set => this.RaiseAndSetIfChanged(ref _isAuthenticated, value);
    }
    
    /// <summary>
    /// Username of the authenticated user
    /// </summary>
    public string? Username
    {
        get => _username;
        private set => this.RaiseAndSetIfChanged(ref _username, value);
    }
    
    private async Task StartAuthAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsAuthenticating = true;
            
            // Use a callback URL that the app can handle
            // For desktop apps, this could be a custom protocol handler
            // For mobile apps, this could be a deep link
            // For now, we'll use a dummy URL
            var callbackUrl = "swipemyroof://auth";
            
            // Start the authentication process
            AuthUrl = await _authService.StartAuthenticationAsync(callbackUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error starting authentication: {ex.Message}");
        }
        finally
        {
            IsAuthenticating = false;
        }
    }
    
    private async Task CompleteAuthAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsAuthenticating = true;
            
            // Complete the authentication process
            var success = await _authService.CompleteAuthenticationAsync(Verifier, cancellationToken);
            
            if (success)
            {
                IsAuthenticated = true;
                Username = _authService.GetUsername();
                Verifier = string.Empty;
                AuthUrl = string.Empty;
            }
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error completing authentication: {ex.Message}");
        }
        finally
        {
            IsAuthenticating = false;
        }
    }
    
    private void SignOut()
    {
        _authService.SignOut();
        IsAuthenticated = false;
        Username = null;
        Verifier = string.Empty;
        AuthUrl = string.Empty;
    }
}