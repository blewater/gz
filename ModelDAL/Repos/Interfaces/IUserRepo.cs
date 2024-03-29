﻿using System;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces {
    public interface IUserRepo {

        ApplicationUser GetCachedUser(int userId);
    }
}