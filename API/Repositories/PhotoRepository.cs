
using API.Data;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public PhotoRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PhotoDto>> GetWaitingForApproval()
    {
        return await _context.Photos
            .Where(p => !p.IsApproved)
            .ProjectTo<PhotoDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<Photo> GetById(int id)
    {
        return await _context.Photos.FindAsync(id);
    }

    public async Task<bool> HasMain(int userId)
    {
        return await _context.Photos
            .AnyAsync(p => p.AppUserId == userId && p.IsMain);
    }

    public void Delete(Photo photo)
    {
        _context.Photos.Remove(photo);
    }
}
