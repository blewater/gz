using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using AutoMapper;
using gzWeb.Models;
using gzWeb.Repo.Interfaces;

namespace gzWeb.Repo {
    public class CustomerRepo : ICustomerRepo
    {
        private readonly UserManager<ApplicationUser, int> manager;

        public CustomerRepo(UserManager<ApplicationUser, int> manager)
        {
            this.manager = manager;
        }
        /// <summary>
        /// Create a customer (user) only if not preexisting. Otherwise, update it.
        /// </summary>
        /// <param name="newUser">The new applicationUser</param>
        /// <param name="password">Password in clear text</param>
        /// <returns>The Id of the new User</returns>
        public int CreateOrUpdateUser(ApplicationUser newUser, string password) {
            
            // Don't recreate if existing
            var fUser = manager.FindByEmail(newUser.Email);
            if (fUser == null) {
                manager.Create(newUser, password);
            } else {
                manager.Update(newUser);
            }
            
            return manager.FindByEmail(newUser.Email).Id;
        }


    }
}

