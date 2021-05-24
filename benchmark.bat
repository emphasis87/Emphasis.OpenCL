dotnet clean "src\Emphasis.OpenCL.sln"
dotnet build -c Release "src\Emphasis.OpenCL.sln"
cd "src\Emphasis.OpenCL.Tests.Benchmarks\"
dotnet run -c Release -- --filter *
cd ..\..
