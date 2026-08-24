using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Email;

namespace Void.Bot.Core.Services.Email;

public interface IEmailDataProvider : IScope
{
    Task<EmailCredential?> GetCredentialAsync(
        bool tracking,
        CancellationToken cancellationToken);

    Task SaveCredentialAsync(
        EmailCredential credential,
        CancellationToken cancellationToken);
}

public sealed class EmailDataProvider(
    IRepository<EmailCredential> repository,
    IUnitOfWork unitOfWork) : IEmailDataProvider
{
    public Task<EmailCredential?> GetCredentialAsync(
        bool tracking,
        CancellationToken cancellationToken)
    {
        return tracking
            ? repository.Query().SingleOrDefaultAsync(cancellationToken)
            : repository.QueryNoTracking().SingleOrDefaultAsync(cancellationToken);
    }

    public async Task SaveCredentialAsync(
        EmailCredential credential,
        CancellationToken cancellationToken)
    {
        var exists = await repository.AnyAsync(
            x => x.Id == credential.Id, cancellationToken).ConfigureAwait(false);
        if (exists)
        {
            await repository.UpdateAsync(credential, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await repository.InsertAsync(credential, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
