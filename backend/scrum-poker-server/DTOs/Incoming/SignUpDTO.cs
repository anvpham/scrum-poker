﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace scrum_poker_server.DTOs.Incoming
{
    public class SignUpDTO
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }
    }
}