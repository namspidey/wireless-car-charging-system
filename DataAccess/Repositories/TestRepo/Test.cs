using DataAccess.Interfaces;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.TestRepo
{
    public class Test : ITest
    {
        private WccsContext _context ;
        public Test() {
            _context = new WccsContext();
        }

        public List<Role> GetAllRoles()
        {
            return _context.Roles.ToList();
        }
    }
}
