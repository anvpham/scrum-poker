﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace scrum_poker_server.DTOs
{
    public class AssignStoryDTO
    {
        public string RoomCode { get; set; }

        public int StoryId { get; set; }

        public string UserName { get; set; }
    }
}