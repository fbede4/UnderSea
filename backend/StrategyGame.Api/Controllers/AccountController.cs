﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrategyGame.Api.DTO;
using StrategyGame.Bll.Services.Country;
using StrategyGame.Dal;
using StrategyGame.Model.Entities;

namespace StrategyGame.Api.Controllers
{
    /// <summary>
    /// API Endpoint for creating accounts and querying the currently logged in user's data.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ICountryService _countryService;

        public AccountController(UserManager<User> userManager, ICountryService countryService)
        {
            _userManager = userManager;
            _countryService = countryService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserData>> GetAccountAsync()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            return Ok(new UserData
            {
                Username = currentUser.UserName,
                Email = currentUser.Email
            });
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> CreateAccountAsnyc([FromBody] RegisterData data)
        {
            if (await _userManager.FindByNameAsync(data.Username) != null)
            {
                return BadRequest("Duplicate username");
            }

            var user = new User()
            {
                UserName = data.Username,
                Email = data.Email
            };

            var result = await _userManager.CreateAsync(user, data.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _countryService.CreateAsync(data.Username, data.CountryName);
            return StatusCode(201, new UserData
            {
                Username = user.UserName,
                Email = user.Email
            });
        }
    }
}