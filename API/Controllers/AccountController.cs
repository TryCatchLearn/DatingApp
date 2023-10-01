using API.Entities;
using API.DTOs;
using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public AccountController(ITokenService tokenService, UserManager<AppUser> userManager, IMapper mapper)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var user = await _userManager.FindByNameAsync(registerDto.UserName.ToLower());
            if (user != null)
            {
                return BadRequest("Username is taken");
            }

            user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.UserName.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            result = await _userManager.AddToRoleAsync(user, "Member");
            if (!result.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users
                .Include(u => u.Photos)
                .FirstOrDefaultAsync(u => u.UserName == loginDto.UserName.ToLower());

            if (user == null) return Unauthorized();

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized();

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
    }

}
