using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UserManagementApp.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
namespace UserManagementApp.Controllers;

public class UserController : Controller
{
    private readonly string apiUrl = "http://localhost:3000"; // Backend API URL
    private readonly ILogger<UserController> _logger;
    private static string _authToken;
    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;

    }

    private bool IsTokenExpired(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        if (jwtHandler.CanReadToken(token))
        {
            var jwtToken = jwtHandler.ReadJwtToken(token);
            var expiry = jwtToken.ValidTo;
            return DateTime.UtcNow >= expiry;
        }

        return true;
    }

    private async Task<string> GetAuthToken()
    {
        if (_authToken == null || IsTokenExpired(_authToken))
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync($"{apiUrl}/auth/token", new { clientId = "frontend-client" });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _authToken = JsonConvert.DeserializeObject<dynamic>(result).token;
                }
                else
                {
                    throw new Exception("Failed to retrieve auth token");
                }
            }
        }
        return _authToken;
    }


    // Display users
    public async Task<ActionResult> Index(string statusFilter, string searchQuery)
    {
        // _logger.LogInformation("StatusFilter: {statusFilter}, SearchQuery: {searchQuery}", statusFilter, searchQuery);

        string url = $"{apiUrl}/users?status={statusFilter}&search={searchQuery}";
        var users = await GetUsersFromApi(url);
        ViewData["statusFilter"] = statusFilter;
        ViewData["searchQuery"] = searchQuery;
        return View(users);
    }

    // get all the users
    public async Task<List<User>> GetUsersFromApi(string url)
    {
        var token = await GetAuthToken();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<User>>(response);
        }
    }

    // render the Create view
    [HttpGet]
    public IActionResult Create()
    {
        return View(); // looks for Views/User/Create.cshtml
    }
    // Create a new user
    [HttpPost]
    public async Task<ActionResult> Create(User newUser)
    {
        string url = $"{apiUrl}/users/create";
        var token = await GetAuthToken();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsJsonAsync(url, newUser);

            _logger.LogInformation("Response: {response}", response);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync(); // get the error message
                ViewData["ErrorMessage"] = errorMessage; // pass the error message to the view
                return View("Create", newUser);

            }
        }
    }

    //edit user
    [HttpPost]
    public async Task<ActionResult> Edit(User user)
    {
        string url = $"{apiUrl}/users/{user.UserID}";
        var token = await GetAuthToken();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.PutAsJsonAsync(url, user);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync(); // get the error message
                ViewData["ErrorMessage"] = errorMessage;
                return View(user); // Return back to edit form on failure
            }
        }
    }

    [HttpGet]
    public async Task<ActionResult> Edit(int id)
    {
        string url = $"{apiUrl}/users/{id}";
        var token = await GetAuthToken();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetStringAsync(url);
            var user = JsonConvert.DeserializeObject<User>(response);
            return View(user);
        }
    }


    // delete a user
    [HttpPost]
    public async Task<ActionResult> Delete(int userID)
    {
        string url = $"{apiUrl}/users/{userID}";
        var token = await GetAuthToken();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");
            else
            {

                var errorMessage = await response.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = errorMessage;
                return View("Index");
            }
        }
    }
}
