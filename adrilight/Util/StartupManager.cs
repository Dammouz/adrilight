using System;
using System.Security.Principal;
using Microsoft.Win32;

public class StartUpManager
{
    private const string ApplicationName = "adrilight";

    public static void AddApplicationToCurrentUserStartup()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            key.SetValue(ApplicationName, $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"");
        }
    }

    public static void AddApplicationToAllUserStartup()
    {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            key.SetValue(ApplicationName, $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"");
        }
    }

    public static void RemoveApplicationFromCurrentUserStartup()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            key.DeleteValue(ApplicationName, false);
        }
    }

    public static void RemoveApplicationFromAllUserStartup()
    {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            key.DeleteValue(ApplicationName, false);
        }
    }

    public static bool IsUserAdministrator()
    {
        // Bool value to hold our return value
        bool isAdmin;
        try
        {
            // Get the currently logged in user
            var user = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(user);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (UnauthorizedAccessException)
        {
            isAdmin = false;
        }
        catch (Exception)
        {
            isAdmin = false;
        }
        return isAdmin;
    }
}
