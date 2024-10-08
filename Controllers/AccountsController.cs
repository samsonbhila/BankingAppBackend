﻿using BankingAppBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using BankingAppBackend.Services.Interfaces;
using System.Collections.Generic;

namespace BankingAppBackend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AccountsController : ControllerBase
    {
        private IMapper _mapper;
        private IUserService _userService;

        public AccountsController(IMapper mapper, IUserService userService)
        {
            _mapper = mapper;
            _userService = userService;
        }
        [HttpPost]
        [Route("register_new_account")]
        public IActionResult RegisterNewAccount([FromBody] RegisterNewAccountModel newAccount)
        {
            if (!ModelState.IsValid) return BadRequest(newAccount);
            //map
            var account = _mapper.Map<Account>(newAccount);
            return Ok(_userService.Create(account, newAccount.Pin, newAccount.ConfirmPin));
        }

        [HttpGet]
        [Route("get_account_by_id")]
        public IActionResult GetAccountById(int Id)
        {
            var account = _userService.GetById(Id);
            var getAccountModel = _mapper.Map<GetAccountModel>(account);
            return Ok(getAccountModel);
        }

        [HttpGet]

        [Route("get_all_accounts")]
        public IActionResult GetAllAccounts()
        {
            var allAccounts = _userService.GetAllAccounts();
            var getCleanedAccounts = _mapper.Map<IList<GetAccountModel>>(allAccounts);
            return Ok(getCleanedAccounts);
        }

        [HttpPost]
        [Route("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            if (!ModelState.IsValid) return BadRequest(model);

            var authResult = _userService.Authenticate(model.Pin);
            if (authResult == null) return Unauthorized("Invalid Pin Please enter your Pin");
            return Ok(authResult);
        }
    }
}
