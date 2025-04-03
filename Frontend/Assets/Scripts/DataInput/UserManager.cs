using UnityEngine;

public class UserManager : MonoBehaviour
{
    private static UserManager _instance;
    
    public static UserManager Instance
    {
        get
        {
            return _instance;
        }
    }
    
    private int _userId = -1; // Changed from string to int with default value -1
    private string _username = "";
    private string _token = ""; // Added token field for future use if needed
    private bool _isLoggedIn = false;
    
    // Public properties to access user information
    public int UserId => _userId; // Changed from string to int
    public string Username => _username;
    public string Token => _token;
    public bool IsLoggedIn => _isLoggedIn;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Implement singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // New method that accepts int userId
    public void SetUserInfo(int userId, string username, string token = "")
    {
        _userId = userId;
        _username = username;
        _token = token;
        _isLoggedIn = true;
        
        Debug.Log($"User logged in - ID: {_userId}, Username: {_username}");
    }
    
    // Keep the original method for backward compatibility
    public void SetUserInfo(string username, string token)
    {
        // If username is actually the userId in string format, try to parse it
        if (int.TryParse(username, out int parsedId))
        {
            _userId = parsedId;
        }
        else
        {
            _userId = -1; // Invalid/unknown ID
        }
        
        _username = username;
        _token = token;
        _isLoggedIn = true;
        
        //Debug.Log($"User logged in - ID: {_userId}, Username: {_username}");
    }
    
    public void Logout()
    {
        //Debug.Log($"User logged out - ID: {_userId}, Username: {_username}");
        
        _userId = -1; // Reset to default value
        _username = "";
        _token = "";
        _isLoggedIn = false;
    }
    
    // Updated to handle int userId correctly when comparing to string creatorId
    public bool IsRaceCreator(string creatorId)
    {
        if (_userId == -1 || string.IsNullOrEmpty(creatorId))
        {
            return false;
        }
        
        // Try to parse creatorId as int for comparison
        if (int.TryParse(creatorId, out int parsedCreatorId))
        {
            return _userId == parsedCreatorId;
        }
        
        return false;
    }
    
    // New overload for directly comparing with int creatorId
    public bool IsRaceCreator(int creatorId)
    {
        if (_userId == -1)
        {
            return false;
        }
        
        return _userId == creatorId;
    }
}