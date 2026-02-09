# Building a .NET Project

This document provides instructions for building a .NET project using the .NET CLI or Visual Studio.

## Prerequisites

- **.NET SDK**: Install the appropriate .NET SDK version for your project (6.0, 7.0, 8.0, etc.)
  - Download from: https://dotnet.microsoft.com/download
  - Verify installation: `dotnet --version`

- **Visual Studio** (optional): Visual Studio 2022 or later for IDE development
- **Git**: For version control operations

## Quick Start

### 1. Restore Dependencies

Before building, restore all NuGet packages:

```bash
dotnet restore
```

### 2. Build the Project

**Debug Configuration:**
```bash
dotnet build
```

**Release Configuration:**
```bash
dotnet build --configuration Release
```

### 3. Clean Previous Builds

Remove build artifacts before rebuilding:

```bash
dotnet clean
```

## Detailed Build Options

### Build with Specific Framework

If your project targets multiple frameworks:

```bash
dotnet build --framework net8.0
```

### Verbose Output

For detailed build information:

```bash
dotnet build --verbosity detailed
```

Verbosity levels: `quiet`, `minimal`, `normal`, `detailed`, `diagnostic`

### Suppress Warnings

Build without showing warnings:

```bash
dotnet build --no-warn
```

### Output to Specific Directory

```bash
dotnet build --output ./bin/custom
```

## Running the Project

### Run in Debug Mode

```bash
dotnet run
```

### Run with Arguments

```bash
dotnet run -- --arg1 value1 --arg2 value2
```

### Run Specific Project

In a solution with multiple projects:

```bash
dotnet run --project ./MyProject/MyProject.csproj
```

## Testing

### Run Tests

```bash
dotnet test
```

### Run Tests with Verbose Output

```bash
dotnet test --verbosity detailed
```

### Run Specific Test Project

```bash
dotnet test ./MyTests/MyTests.csproj
```

## Publishing

### Create Release Build

```bash
dotnet publish --configuration Release
```

### Publish for Specific Runtime

```bash
dotnet publish --configuration Release --runtime win-x64
```

### Create Self-Contained Executable

```bash
dotnet publish --configuration Release --self-contained
```

## Common Issues & Troubleshooting

### "Project file not found"
- Ensure you're running commands from the correct directory
- Verify the project file (`.csproj`, `.vbproj`, etc.) exists

### "Unsupported or unrecognized format for the specified assembly"
- Check .NET SDK and project compatibility
- Run: `dotnet --info` to verify SDK version

### NuGet Package Errors
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Restore packages: `dotnet restore`
- Check package sources: `dotnet nuget list source`

### Build Fails with Framework Not Installed
- Install missing framework: `dotnet --list-sdks` and `dotnet --list-runtimes`
- Download required version from https://dotnet.microsoft.com/download

## Useful Commands Reference

| Command | Description |
|---------|-------------|
| `dotnet --version` | Check installed SDK version |
| `dotnet --info` | Show detailed SDK and runtime information |
| `dotnet new list` | List available project templates |
| `dotnet sln list` | List all projects in a solution |
| `dotnet add package <name>` | Add NuGet package |
| `dotnet remove package <name>` | Remove NuGet package |
| `dotnet format` | Format code (requires dotnet-format tool) |

## Building in CI/CD

### GitHub Actions Example

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v3
  with:
    dotnet-version: '8.0.x'

- name: Restore
  run: dotnet restore

- name: Build
  run: dotnet build --configuration Release --no-restore

- name: Test
  run: dotnet test --configuration Release --no-build
```

### Azure Pipelines Example

```yaml
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- task: DotNetCoreCLI@2
  inputs:
    command: 'restore'

- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    arguments: '--configuration Release'
```

## Performance Tips

1. **Use Release Configuration** for production builds
2. **Parallel Compilation**: Builds use multiple cores by default
3. **Incremental Builds**: Only rebuild changed files when possible
4. **Cache NuGet Packages**: Store packages in a shared location for team development
5. **Clean Periodically**: Run `dotnet clean` if you encounter persistent issues

## Additional Resources

- [Microsoft .NET Documentation](https://learn.microsoft.com/dotnet/)
- [dotnet CLI Reference](https://learn.microsoft.com/dotnet/core/tools/)
- [NuGet Package Manager](https://www.nuget.org/)
- [.NET SDK Releases](https://github.com/dotnet/release-notes)
