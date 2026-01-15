using Microsoft.AspNetCore.Mvc;
using JobModels;
using JobWebService.ORM.Repositories;
using System.Diagnostics;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        LibraryUOW libraryUOW;
        [HttpPost]
        public bool Register(User user)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.Create(user);
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpPost]
        [ActionName("UpdateUser")]
        public bool UpdateUser(User user)
        {
           
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return libraryUOW.UserRepository.Update(user);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
            return false;
        }
        [HttpPost]
        [ActionName("UpdatePassword")]
        public bool UpdatePassword(User user)
        {
            
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return libraryUOW.UserRepository.UpdatePassword(user);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
            return false;
        }

        [HttpGet]
        public bool CheckPassword(string userId, string password)
        {
            
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                string password2 = libraryUOW.UserRepository.GetPasswordByUserId(userId);
                return password2.Equals(password);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
            return false;
        }

        [HttpGet]
        public bool IsAvailableUserName(string username)
        {
            
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return !libraryUOW.UserRepository.IsExistUserName(username);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
            return false;
        }
    }
}
