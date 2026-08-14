using Mediator.Net;
using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Constants.Security;

namespace Void.Bot.IntegrationTest.Services.Identity;

public class IdentityFixture(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    [Fact]
    public async Task RegisterUserAccountAsync()
    {
        await Run<IMediator>(async mediator =>
        {
            var userName = "test";
            
            var response = await mediator.SendAsync<RegisterUserAccountCommand, RegisterUserAccountResponse>(
                new RegisterUserAccountCommand
                {
                    Name = userName,
                    Password = StringExtension.GenerateRandomPassword()
                }).ConfigureAwait(false);
            
            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Name.ShouldBe(userName);
        });
    }

    [Fact]
    public async Task RegisterUserAccountAsync_AssignsSuperAdminOnlyToFirstUser()
    {
        await Run<IMediator, IRepository<UserRole>>(async (mediator, userRoleRepository) =>
        {
            var first = await mediator.SendAsync<RegisterUserAccountCommand, RegisterUserAccountResponse>(
                new RegisterUserAccountCommand
                {
                    Name = "first-user",
                    Password = StringExtension.GenerateRandomPassword()
                }).ConfigureAwait(false);
            var second = await mediator.SendAsync<RegisterUserAccountCommand, RegisterUserAccountResponse>(
                new RegisterUserAccountCommand
                {
                    Name = "second-user",
                    Password = StringExtension.GenerateRandomPassword()
                }).ConfigureAwait(false);

            first.Data.ShouldNotBeNull();
            second.Data.ShouldNotBeNull();
            (await userRoleRepository.AnyAsync(
                x => x.UserId == first.Data.Id && x.RoleId == RoleConstants.SuperAdminId)).ShouldBeTrue();
            (await userRoleRepository.AnyAsync(
                x => x.UserId == second.Data.Id && x.RoleId == RoleConstants.SuperAdminId)).ShouldBeFalse();
        });
    }
}