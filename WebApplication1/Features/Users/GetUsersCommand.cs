using GuidesApi.Data;
using GuidesApi.Features.Users.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GuidesApi.Features.Users
{
    public class GetUsersCommand : IRequest<List<UserModel>>
    {
        public string Id { get; set; }
    }

    public class GetUsersCommandHander : IRequestHandler<GetUsersCommand, List<UserModel>>
    {
        private readonly AppDbContext _context;

        public GetUsersCommandHander(AppDbContext context) => _context = context;

        public async Task<List<UserModel>> Handle(GetUsersCommand request, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(x => x.Id == (request.Id ?? x.Id))
                .Select(x => new UserModel
                {
                    Email = x.Email,
                    FirstName = x.FirstName,
                    Id = x.Id,
                    LastName = x.LastName,
                    Password = x.Password
                })
                .ToListAsync(cancellationToken);
        }
    }
}
