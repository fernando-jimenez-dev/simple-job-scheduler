using Application.JobsUseCases.ExecutePowerShell.Errors;
using FluentValidation.Results;
using Shouldly;
using WebAPI.Minimal.Shared;
using WebAPI.Minimal.JobsUseCases.ExecutePowerShell;

namespace WebAPI.Minimal.UnitTests.Shared;

public class ApiErrorTests
{
    [Fact]
    public void ShouldCreateApiError_FromApplicationError_AndAddRequestDetail()
    {
        var error = new FileNotFoundError(new("C:\\fake\\script.ps1"));
        var request = new ExecutePowerShellRequest("C:\\fake\\script.ps1");

        var apiError = ApiError.FromApplicationError(error, request);

        apiError.Id.ShouldBe(error.Id);
        apiError.Type.ShouldBe(error.Type);
        apiError.Message.ShouldBe(error.Message);
        apiError.Details.ShouldContainKey("request");
        apiError.Details["request"].ShouldBe(request);
    }

    [Fact]
    public void ShouldAddDetail_WhenWithDetailIsCalled()
    {
        var error = new FileIsNotPowerShellError(new("C:\\path\\invalid.txt"));
        var request = new ExecutePowerShellRequest("C:\\path\\invalid.txt");

        var apiError = ApiError.FromApplicationError(error, request);
        apiError = apiError.WithDetail("extraInfo", "some extra context");

        apiError.Details.ShouldContainKey("extraInfo");
        apiError.Details["extraInfo"].ShouldBe("some extra context");
    }

    [Fact]
    public void ShouldOverwriteDetail_WhenWithDetailIsCalledTwiceWithSameKey()
    {
        var validationResult = new ValidationResult([
            new ValidationFailure("property", "invalid value")
        ]);
        var error = new InvalidInputError(new("C:\\something"), validationResult);
        var request = new ExecutePowerShellRequest("C:\\something");

        var apiError = ApiError.FromApplicationError(error, request);
        apiError = apiError
            .WithDetail("request", "InitialValue")
            .WithDetail("request", "OverwrittenValue");

        apiError.Details["request"].ShouldBe("OverwrittenValue");
    }
}