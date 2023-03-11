using GuidesApi.Data;
using GuidesApi.Data.Models;
using GuidesApi.Features.Users.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GuidesApi.Features.Users
{
    public class CreateEditUserCommand : IRequest<string>
    {
        public UserModel User { get; set; }
    }

    public class CreateEditUserCommandHandler : IRequestHandler<CreateEditUserCommand, string>
    {
        private readonly AppDbContext _context;
        public CreateEditUserCommandHandler(AppDbContext context) => _context = context;

        public async Task<string> Handle(CreateEditUserCommand request, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.User.Id, cancellationToken) ?? new();

            user.Id = request.User.Id;
            user.FirstName = request.User.FirstName;
            user.LastName = request.User.LastName;
            user.Email = request.User.Email;
            user.Password = request.User.Password;
            user.IsDeleted = request.User.IsDeleted;

            if (!await _context.Users.AnyAsync(x => x.Id == request.User.Id, cancellationToken))
            {
                await _context.Users.AddAsync(user, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}
