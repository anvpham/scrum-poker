﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using scrum_poker_server.Data;
using scrum_poker_server.DTOs.Incoming;
using scrum_poker_server.Models;
using scrum_poker_server.Utils.Jwt;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace scrum_poker_server.Controllers
{
    [ApiController]
    [Route("api/signup")]
    public class SignUpController : ControllerBase
    {
        public AppDbContext _dbContext { get; set; }

        public IConfiguration _configuration { get; set; }

        public JwtTokenGenerator JwtTokenGenerator { get; set; }

        public SignUpController(AppDbContext dbContext, IConfiguration configuration, JwtTokenGenerator jwtTokenGenerator)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            JwtTokenGenerator = jwtTokenGenerator;
        }

        [Consumes("application/json")]
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO data)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(data.Email))
                {
                    var anonymousUser = new User()
                    {
                        Name = data.Username
                    };

                    await _dbContext.Users.AddAsync(anonymousUser);
                    await _dbContext.SaveChangesAsync();

                    return Ok(new { token = JwtTokenGenerator.GenerateToken(new UserData { UserId = anonymousUser.Id }) });
                }

                bool isEmailExisted = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == data.Email) != null;
                if (isEmailExisted) return StatusCode(409, new { error = "The email is already existed" });

                // Compute hash of the password
                var bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(data.Password));
                string hashedPassword = BitConverter.ToString(bytes).Replace("-", "").ToLower();

                var user = new User()
                {
                    Email = data.Email,
                    Password = hashedPassword,
                    Name = data.Username,
                    Account = new Account()
                };

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                return Ok(new { token = JwtTokenGenerator.GenerateToken(new UserData { Email = data.Email, UserId = user.Id }), userName = data.Username, userId = user.Id });
            }
            else return StatusCode(422);
        }
    }
}
