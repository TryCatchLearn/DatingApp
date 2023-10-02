using API.DTOs;

namespace API.Interfaces;

public interface IPhotoRepository
{
    public Task<List<PhotoDto>> GetWaitingForApproval();
    public Task<Photo> GetById(int id);
    public void Delete(Photo photo);
    public Task<bool> HasMain(int userId);
}
