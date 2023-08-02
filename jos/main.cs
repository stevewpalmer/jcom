using JComLib;

Console.WriteLine("JOs v1.0");

while (true) {
    Console.Write(">");
    ReadLine readLine = new() {
        AllowHistory = true
    };
    string inputLine = readLine.Read(string.Empty);
    if (inputLine == null) {
        continue;
    }
}
