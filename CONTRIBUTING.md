# Contributing to EasyHealth.HealthChecks

Thank you for your interest in contributing to EasyHealth.HealthChecks! This document provides guidelines and information for contributors.

## 🤝 How to Contribute

### Reporting Issues

1. **Check existing issues** first to avoid duplicates
2. **Use the issue template** when creating new issues
3. **Provide detailed information** including:
   - .NET version
   - Package version
   - Steps to reproduce
   - Expected vs actual behavior
   - Error messages or logs

### Submitting Changes

1. **Fork the repository**
2. **Create a feature branch** from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes** following our coding standards
4. **Write tests** for new functionality
5. **Update documentation** as needed
6. **Submit a pull request**

## 🏗️ Development Setup

### Prerequisites

- .NET 8 SDK or later
- .NET 9 SDK (for multi-targeting)
- Git
- A code editor (VS Code, Visual Studio, Rider, etc.)

### Building the Project

```bash
# Clone the repository
git clone https://github.com/dbbuilder/easyhealthcheck.git
cd easyhealthcheck

# Restore dependencies
dotnet restore EasyHealth.HealthChecks/EasyHealth.HealthChecks.csproj

# Build the project
dotnet build EasyHealth.HealthChecks/EasyHealth.HealthChecks.csproj

# Run tests (when available)
dotnet test

# Create a test application
dotnet run --project TestProject/TestProject.csproj
```

## 📝 Coding Standards

### C# Style Guidelines

- Follow standard C# naming conventions
- Use `async/await` for asynchronous operations
- Add XML documentation comments for public APIs
- Use nullable reference types appropriately
- Keep methods focused and single-purpose

### Example Code Style

```csharp
/// <summary>
/// Checks the health of the specified service.
/// </summary>
/// <param name="context">The health check context.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The health check result.</returns>
public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context, 
    CancellationToken cancellationToken = default)
{
    try
    {
        var result = await DoHealthCheckAsync(cancellationToken);
        return HealthCheckResult.Healthy("Service is healthy", result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Health check failed for {ServiceName}", context.Registration.Name);
        return HealthCheckResult.Unhealthy("Service is unhealthy", ex);
    }
}
```

## 🧪 Testing Guidelines

### Test Structure

- Use xUnit for testing framework
- Follow AAA pattern (Arrange, Act, Assert)
- Use descriptive test method names
- Test both success and failure scenarios
- Mock external dependencies

### Example Test

```csharp
[Fact]
public async Task CheckHealthAsync_WhenServiceIsHealthy_ReturnsHealthyResult()
{
    // Arrange
    var mockService = new Mock<IExternalService>();
    mockService.Setup(x => x.IsHealthyAsync()).ReturnsAsync(true);
    var healthCheck = new ServiceHealthCheck(mockService.Object);

    // Act
    var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

    // Assert
    Assert.Equal(HealthStatus.Healthy, result.Status);
}
```

## 📚 Documentation

### README Updates

When adding new features, update the README.md with:
- Configuration examples
- Usage instructions
- API documentation
- Breaking changes (if any)

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Configures memory health check monitoring.
/// </summary>
/// <param name="maxMemoryMB">Maximum memory usage in MB before reporting unhealthy.</param>
/// <param name="name">Optional name for the health check.</param>
/// <returns>Configuration instance for method chaining.</returns>
public EasyHealthConfiguration AddMemoryCheck(int maxMemoryMB = 1024, string? name = null)
```

## 🔄 Pull Request Process

### Before Submitting

1. **Ensure your code builds** without warnings
2. **Run all tests** and ensure they pass
3. **Update documentation** for any API changes
4. **Add tests** for new functionality
5. **Follow semantic versioning** for breaking changes

### Pull Request Template

When submitting a PR, include:

- **Description** of changes made
- **Motivation** for the changes
- **Testing** performed
- **Breaking changes** (if any)
- **Related issues** (link with #issue-number)

### Example PR Description

```markdown
## Description
Added support for Redis health checks with configurable connection timeout.

## Motivation
Users requested the ability to monitor Redis cache availability.

## Changes
- Added `RedisHealthCheck` class
- Added `AddRedisCheck()` configuration method  
- Added comprehensive tests
- Updated documentation

## Testing
- Unit tests for all new functionality
- Integration tests with Redis container
- Manual testing with sample application

## Breaking Changes
None

## Related Issues
Closes #42
```

## 🎯 Contribution Areas

We welcome contributions in these areas:

### High Priority
- Additional health check implementations
- Performance improvements
- Better error handling
- Documentation improvements

### Medium Priority  
- Integration with monitoring systems
- Custom health check templates
- Configuration validation
- Metrics collection

### Low Priority
- UI dashboard (separate package)
- Additional output formats
- Localization support

## 🚀 Release Process

### Versioning Strategy

We follow [Semantic Versioning](https://semver.org/):
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Checklist

1. Update version in `.csproj` file
2. Update CHANGELOG.md
3. Create release notes
4. Tag the release
5. Publish to NuGet via GitHub Actions

## ❓ Questions?

If you have questions about contributing:

1. Check the [Wiki](https://github.com/dbbuilder/easyhealthcheck/wiki)
2. Search [existing issues](https://github.com/dbbuilder/easyhealthcheck/issues)
3. Start a [discussion](https://github.com/dbbuilder/easyhealthcheck/discussions)
4. Contact the maintainers

## 🙏 Recognition

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- Package acknowledgments

Thank you for helping make EasyHealth.HealthChecks better! 🎉