using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using AutoMapper;

namespace gzWeb.Models {
    public class CustomerRepo {

        /// <summary>
        /// Create a customer (user) only if not preexisting. Otherwise, update it.
        /// </summary>
        /// <param name="dto">Use the VM object since the applicationUser has non-public properties i.e. Password (clear)</param>
        /// <returns></returns>
        public int CreateUpdUser(CustomerDTO dto) {

            using (var db = new ApplicationDbContext()) {

                var manager = new ApplicationUserManager(new CustomUserStore(db));
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
}

