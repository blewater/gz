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
        /// <param name="dto">Use the VM object since the applicationUser has non-public properties i.e. Password (clear)</param>
        /// <returns></returns>
        public int CreateUpdUser(CustomerDTO dto) {
            
            var newUser = new ApplicationUser();

            Mapper.Initialize(cfg => cfg.CreateMap<CustomerDTO, ApplicationUser>());
            //Mapper.CreateMap <CustomerDTO, ApplicationUser> ();
            Mapper.Map<CustomerDTO, ApplicationUser>(dto, newUser);

            // Don't recreate if existing
            var fUser = manager.FindByEmail(newUser.Email);
            if (fUser == null) {
                manager.Create(newUser, dto.Password);
            } else {
                manager.Update(newUser);
            }

            var custId = manager.FindByEmail(newUser.Email).Id;
            return custId;
            
        }


    }
}

