using Shouldly;
using Void.Bot.Core.Validators.Account;
using Void.Bot.Core.Validators.Identity;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Enum;

namespace Void.Bot.UnitTest.Identity;

public class AccountValidatorFixture
{
    [Theory]
    [InlineData("a")]
    [InlineData(" ")]
    [InlineData("用户 名称!#")]
    public void RegistrationShouldAcceptAnyNameBetweenOneAndSixtyFourCharacters(string name)
    {
        var password = "Password1";

        new RegisterUserAccountCommandValidator().Validate(new RegisterUserAccountCommand
        {
            Name = name,
            Password = password
        }).IsValid.ShouldBeTrue();

        new OAuthRegisterCommandValidator().Validate(new OAuthRegisterCommand
        {
            Code = "registration-code",
            Name = name,
            Password = password
        }).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void RegistrationShouldAcceptSixtyFourCharacterNames()
    {
        var name = new string('名', 64);
        var password = "Password1";

        new RegisterUserAccountCommandValidator().Validate(new RegisterUserAccountCommand
        {
            Name = name,
            Password = password
        }).IsValid.ShouldBeTrue();

        new OAuthRegisterCommandValidator().Validate(new OAuthRegisterCommand
        {
            Code = "registration-code",
            Name = name,
            Password = password
        }).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65)]
    public void RegistrationShouldRejectNamesOutsideTheLengthLimit(int length)
    {
        var name = new string('a', length);
        var password = "Password1";

        new RegisterUserAccountCommandValidator().Validate(new RegisterUserAccountCommand
        {
            Name = name,
            Password = password
        }).IsValid.ShouldBeFalse();

        new OAuthRegisterCommandValidator().Validate(new OAuthRegisterCommand
        {
            Code = "registration-code",
            Name = name,
            Password = password
        }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ChangePasswordShouldRequireValidMatchingNewPassword()
    {
        var validator = new ChangeUserAccountPasswordCommandValidator();

        validator.Validate(new ChangeUserAccountPasswordCommand
        {
            OldPassword = "OldPassword1",
            NewPassword = "NewPassword1",
            NewPasswordConfirm = "NewPassword1"
        }).IsValid.ShouldBeTrue();

        validator.Validate(new ChangeUserAccountPasswordCommand
        {
            OldPassword = "OldPassword1",
            NewPassword = "weak",
            NewPasswordConfirm = "different"
        }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void TwoFactorCommandsRequirePasswordAndSixDigitCode()
    {
        new BeginTwoFactorSetupCommandValidator().Validate(new BeginTwoFactorSetupCommand
        {
            Password = "Password1"
        }).IsValid.ShouldBeTrue();

        new EnableTwoFactorCommandValidator().Validate(new EnableTwoFactorCommand
        {
            Code = "123456"
        }).IsValid.ShouldBeTrue();

        new DisableTwoFactorCommandValidator().Validate(new DisableTwoFactorCommand
        {
            Password = "Password1",
            Code = "123456"
        }).IsValid.ShouldBeTrue();

        new EnableTwoFactorCommandValidator().Validate(new EnableTwoFactorCommand
        {
            Code = "12345a"
        }).IsValid.ShouldBeFalse();

        new VerifyTwoFactorCommandValidator().Validate(new VerifyTwoFactorCommand
        {
            Code = "123456"
        }).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ForceResetPasswordShouldRequireUserAndValidMatchingPassword()
    {
        var validator = new ForceResetUserAccountPasswordCommandValidator();

        validator.Validate(new ForceResetUserAccountPasswordCommand
        {
            UserId = Guid.NewGuid(),
            NewPassword = "NewPassword1",
            NewPasswordConfirm = "NewPassword1"
        }).IsValid.ShouldBeTrue();

        validator.Validate(new ForceResetUserAccountPasswordCommand
        {
            UserId = Guid.Empty,
            NewPassword = "weak",
            NewPasswordConfirm = "different"
        }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ForceUnlinkShouldRequireUserAndThirdPartyPlatform()
    {
        var validator = new ForceUnlinkUserAccountThirdPartyCommandValidator();

        validator.Validate(new ForceUnlinkUserAccountThirdPartyCommand
        {
            UserId = Guid.NewGuid(),
            Platform = AppPlatformEnum.Discord
        }).IsValid.ShouldBeTrue();

        validator.Validate(new ForceUnlinkUserAccountThirdPartyCommand
        {
            UserId = Guid.Empty,
            Platform = AppPlatformEnum.Void
        }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void PasswordResetLinkCommandsRequireValidInputs()
    {
        new SendUserAccountPasswordResetLinkCommandValidator().Validate(new SendUserAccountPasswordResetLinkCommand
        {
            UserId = Guid.NewGuid()
        }).IsValid.ShouldBeTrue();

        new ResetUserAccountPasswordByLinkCommandValidator().Validate(new ResetUserAccountPasswordByLinkCommand
        {
            Token = new string('a', 43),
            NewPassword = "NewPassword1",
            NewPasswordConfirm = "NewPassword1"
        }).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("a", true)]
    [InlineData("用户名!#", true)]
    [InlineData(" ", false)]
    [InlineData("a b", false)]
    [InlineData("a\tb", false)]
    [InlineData("", false)]
    public void ChangeNameShouldUseRegistrationNameRules(string name, bool isValid)
    {
        var result = new ChangeUserAccountNameCommandValidator().Validate(new ChangeUserAccountNameCommand
        {
            Name = name
        });

        result.IsValid.ShouldBe(isValid);
    }

    [Fact]
    public void ChangeNameShouldRejectNamesLongerThanSixtyFourCharacters()
    {
        var result = new ChangeUserAccountNameCommandValidator().Validate(new ChangeUserAccountNameCommand
        {
            Name = new string('a', 65)
        });

        result.IsValid.ShouldBeFalse();
    }
}
