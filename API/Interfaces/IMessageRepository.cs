using API.DTOs;
using API.Entities;
using API.Helpers;
using Newtonsoft.Json.Linq;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName); 
        Task<bool> SaveAllAsync();
    }
}