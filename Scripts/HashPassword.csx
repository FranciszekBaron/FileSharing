// HashPassword.csx
#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

Console.Write("Podaj hasło: ");
var password = Console.ReadLine();
var hash = BCrypt.Net.BCrypt.HashPassword(password); // ✅ BCrypt.Net.BCrypt, nie BCrypt
Console.WriteLine($"\nHash:\n{hash}");