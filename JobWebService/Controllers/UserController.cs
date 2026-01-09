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

        [HttpPost]
        public bool Register(User user)
        {
            DBHelperOledb helperOledb = new DBHelperOledb();
            LibraryUOW libraryUOW = new LibraryUOW(helperOledb);
            try
            {
                helperOledb.OpenConnection();
                user.Password = new Passsword(user.Password);
                user.CreationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                return libraryUOW.UserRepository.Insert(user);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                helperOledb.CloseConnection();
            }
            return false;
        }

        [HttpPost]
        [ActionName("UpdateUser")]
        public bool UpdateUser(User user)
        {
            DBHelperOledb helperOledb = new DBHelperOledb();
            LibraryUOW libraryUOW = new LibraryUOW(helperOledb);
            try
            {
                helperOledb.OpenConnection();
                return libraryUOW.UserRepository.Update(user);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                helperOledb.CloseConnection();
            }
            return false;
        }
        [HttpPost]
        [ActionName("UpdatePassword")]
        public bool UpdatePassword(User user)
        {
            DBHelperOledb helperOledb = new DBHelperOledb();
            LibraryUOW libraryUOW = new LibraryUOW(helperOledb);
            try
            {
                helperOledb.OpenConnection();
                user.Password = new Passsword(user.Password);
                return libraryUOW.UserRepository.UpdatePassword(user);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                helperOledb.CloseConnection();
            }
            return false;
        }

        [HttpGet]
        public bool CheckPassword(string userId, string password)
        {
            DBHelperOledb helperOledb = new DBHelperOledb();
            LibraryUOW libraryUOW = new LibraryUOW(helperOledb);
            try
            {
                helperOledb.OpenConnection();
                Password password = libraryUOW.UserRepository.GetPasswordByUserId(userId);
                return Password.IsMatch(password);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                helperOledb.CloseConnection();
            }
            return false;
        }

        [HttpGet]
        public bool IsAvailableUserName(string username)
        {
            DBHelperOledb helperOledb = new DBHelperOledb();
            LibraryUOW libraryUOW = new LibraryUOW(helperOledb);
            try
            {
                helperOledb.OpenConnection();
                return !libraryUOW.UserRepository.IsExistUserName(username);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                helperOledb.CloseConnection();
            }
            return false;
        }
    }
}


