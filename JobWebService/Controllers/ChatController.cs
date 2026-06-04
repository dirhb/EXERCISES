using JobModels;
using JobWebService.ORM.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ChatController : ControllerBase
    {
        LibraryUOW libraryUOW;

        public ChatController()
        {
            this.libraryUOW = new LibraryUOW();
        }

        [HttpPost]
        public bool SendMessage([FromBody] ChatMessage message)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.ChatMessageRepository.Create(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                return false;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public List<ChatMessage> GetChatHistory(string user1, string user2)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.ChatMessageRepository.ReadHistory(user1, user2);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                return new List<ChatMessage>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public List<User> GetConversations(string userId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                List<string> partnerIds = this.libraryUOW.ChatMessageRepository.ReadConversationPartnerIds(userId);
                List<User> users = new List<User>();

                foreach (string partnerId in partnerIds)
                {
                    User? user = this.libraryUOW.UserRepository.Read(partnerId);
                    if (user != null)
                    {
                        users.Add(user);
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                return new List<User>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }
    }
}
