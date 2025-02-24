using DataAccess.Interfaces;
using DataAccess.Models;

namespace API.Services
{
    public class TestService
    {
        private readonly ITest _test; 

        public TestService(ITest test) 
        {
            _test = test;
        }

        public List<Role> GetAllRoles()
        {
            return _test.GetAllRoles(); 
        }
    }

}
