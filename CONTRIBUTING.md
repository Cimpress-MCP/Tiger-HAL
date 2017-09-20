# Tiger.Hal

This project is using the standard [`dotnet`] build tool. A brief primer:

[`dotnet`]: https://dot.net

- Restore NuGet dependencies: `dotnet restore`
- Build the entire solution: `dotnet build`
- Run all unit tests: `dotnet test ./unit/Test.csproj`
- Pack for publishing: `dotnet pack -o "$(pwd)/artifacts"`

The parameter `--configuration` (shortname `-c`) can be supplied to the `build`, `test`, and `pack` steps with the following meaningful values:

- “Debug” (the default)
- “Release”

This repository is attempting to use the [GitFlow] branching methodology. Results may be mixed, please be aware.

[GitFlow]: http://jeffkreeftmeijer.com/2010/why-arent-you-using-git-flow/

Feature requests are preferred over merge requests.
