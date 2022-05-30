using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
[Authorize]
    public class VisitsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        public VisitsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddVisit(string username)
        {
            var sourceUserId = User.GetUserId();
            var visitedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _unitOfWork.VisitsRepository.GetUserWithVisits(sourceUserId);

            if (visitedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot visit yourself");

            var userVisit = await _unitOfWork.VisitsRepository.GetUserVisit(sourceUserId, visitedUser.Id);

            if (userVisit != null) return BadRequest("You already like this user");

            userVisit = new UserVisit
            {
                SourceUserId = sourceUserId,
                VisitedUserId = visitedUser.Id
            };

            sourceUser.VisitedUsers.Add(userVisit);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to visit user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VisitDto>>> GetUserVisits([FromQuery] VisitsParams visitsParams)
        {
            VisitsParams.UserId = User.GetUserId();
            var users = await _unitOfWork.VisitsRepository.GetUserVisits(visitsParams);

            Response.AddPaginationHeader(users.CurrentPage,
                users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}