// See https://aka.ms/new-console-template for more information

using ZaloOA;

Console.WriteLine("Hello, World!");

var code_verifier = "1234567890asdfghjkl;QWERTYUIOPZXCVBNM<>?123";
var code_challenge = Crypto.SHA256(code_verifier);

Console.WriteLine(code_challenge);

