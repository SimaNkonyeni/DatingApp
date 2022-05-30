using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class VisitsRepository: IVisitsRepository
    {
        private readonly DataContext _context;
        public VisitsRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserVisit> GetUserVisit(int sourceUserId, int visitedUserId)
        {
            return await _context.Visits.FindAsync(sourceUserId, visitedUserId);
        }

        public async Task<PagedList<VisitDto>> GetUserVisits(VisitsParams visitsParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var visits = _context.Visits.AsQueryable();

            if (visitsParams.Predicate == "Visited")
            {
                visits = visits.Where(visit => visit.SourceUserId == visitsParams.UserId);
                users = visits.Select(visit => visit.VisitedUser);
            }

            if (visitsParams.Predicate == "VisitedBy")
            {
                visits = visits.Where(visit => visit.VisitedUserId == visitsParams.UserId);
                users = visits.Select(visit => visit.SourceUser);
            }

            var visitedUsers = users.Select(user => new VisitDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.Id
            });

            return await PagedList<VisitDto>.CreateAsync(visitedUsers, 
                visitsParams.PageNumber, visitsParams.PageSize);
        }

        public async Task<AppUser> GetUserWithVisits(int userId)
        {
            return await _context.Users
                .Include(x => x.VisitedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}