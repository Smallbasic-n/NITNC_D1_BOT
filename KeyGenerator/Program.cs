// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;

Console.WriteLine("Hello, World!");
using var aes = Aes.Create();
aes.KeySize = 256;
aes.GenerateKey();
aes.GenerateIV();
var key = Convert.ToBase64String(aes.Key);
var iv = Convert.ToBase64String(aes.IV);
Console.WriteLine("AES Key");
Console.WriteLine(key);
Console.WriteLine("AES IV");
Console.WriteLine(iv);